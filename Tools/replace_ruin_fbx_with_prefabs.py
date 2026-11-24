"""Replace Ruin scene FBX instances with the pack's collider prefabs.

Run from the Unity project root. This edits Unity YAML while preserving scene
instance IDs and overrides. Missing Buttress wrappers are generated from the
pack's ChurchFrontage collider prefab.
"""

from __future__ import annotations

import hashlib
import re
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PACK = ROOT / "Assets/_MyPackages/Fallen Photoscanned Ruins Pack"
SCENE = ROOT / "Assets/Scenes/Ruin_Arena_FromDemo01.unity"
MODEL_GUID = "df992604d7acb6c4797b8a2213dd3794"
MODEL_TEMPLATE = PACK / "_Prefabs/ChurchFrontage.prefab"


def guid(path: Path) -> str:
    return re.search(r"^guid: ([0-9a-f]{32})", path.with_name(path.name + ".meta").read_text(), re.M).group(1)


def blocks(text: str) -> list[str]:
    return re.split(r"(?=--- !u!)", text)


def wrapper_ids(text: str, model_meta: str) -> dict[str, str]:
    result: dict[str, str] = {}
    sections = blocks(text)
    by_go: dict[str, dict[str, str]] = {}
    go_names: dict[str, str] = {}
    source_names = dict(re.findall(r"^    (\d+): (.+)$", model_meta, re.M))
    for section in sections:
        header = re.match(r"--- !u!(\d+) &(-?\d+)", section)
        if not header:
            continue
        kind, fid = header.groups()
        go = re.search(r"m_GameObject: \{fileID: (-?\d+)\}", section)
        if go:
            by_go.setdefault(go.group(1), {})[kind] = fid
        if kind == "1":
            name = re.search(r"^  m_Name: (.+)$", section, re.M)
            if name:
                go_names[fid] = name.group(1)
        if kind == "4" and "m_Father: {fileID: 0}" in section:
            result["400000"] = fid
            result["100000"] = go.group(1)

    root_go = result["100000"]
    for source_id, name in source_names.items():
        if not (100000 <= int(source_id) < 200000) or source_id == "100000":
            continue
        candidates = [go for go, n in go_names.items() if go != root_go and n.lower() == name.lower()]
        if len(candidates) != 1:
            continue
        go = candidates[0]
        suffix = int(source_id) - 100000
        result[source_id] = go
        for base, kind in ((400000, "4"), (2300000, "23"), (3300000, "33")):
            component = by_go[go].get(kind)
            if component:
                result[str(base + suffix)] = component
    for base, kind in ((2300000, "23"), (3300000, "33"), (20500000, "205")):
        component = by_go[root_go].get(kind) or next((v[kind] for v in by_go.values() if kind in v), None)
        if component:
            result[str(base)] = component
    return result


def make_buttress_wrapper(stem: str, model_guid: str) -> tuple[Path, str, str]:
    template = MODEL_TEMPLATE.read_text(encoding="utf-8-sig")
    output = PACK / "_Prefabs" / f"{stem}.prefab"
    if output.exists():
        return output, guid(output), output.read_text(encoding="utf-8-sig")
    new_guid = hashlib.md5(f"FPS-game-collider-{stem}".encode()).hexdigest()
    old_ids = re.findall(r"^--- !u!\d+ &(-?\d+)", template, re.M)
    id_map = {old: str(8000000000000000 + i) for i, old in enumerate(old_ids, 1)}
    text = re.sub(r"(?<=&)(-?\d+)(?=\n)", lambda m: id_map.get(m.group(1), m.group(1)), template)
    text = re.sub(r"(?<=fileID: )(-?\d+)", lambda m: id_map.get(m.group(1), m.group(1)), text)
    text = text.replace("m_Name: ChurchFrontage\n", f"m_Name: {stem}\n")
    text = text.replace(MODEL_GUID, model_guid)
    return output, new_guid, text


def main() -> None:
    scene = SCENE.read_text(encoding="utf-8-sig")
    prefab_sections = blocks(scene)
    model_paths = {}
    for meta in (PACK / "_Meshes").rglob("*.FBX.meta"):
        model_paths[re.search(r"^guid: ([0-9a-f]{32})", meta.read_text(), re.M).group(1)] = meta.with_suffix("")

    counts: Counter[str] = Counter()
    needed: dict[str, tuple[str, str, dict[str, str]]] = {}
    new_wrappers: dict[Path, tuple[str, str]] = {}
    for section in prefab_sections:
        if not section.startswith("--- !u!1001 &"):
            continue
        source = re.search(r"m_SourcePrefab: \{fileID: 100100000, guid: ([0-9a-f]{32}), type: 3\}", section)
        if not source or source.group(1) not in model_paths:
            continue
        old_guid = source.group(1)
        stem = model_paths[old_guid].stem
        wrapper = PACK / "_Prefabs" / f"{stem}.prefab"
        if wrapper.exists():
            new_guid, wrapper_text = guid(wrapper), wrapper.read_text(encoding="utf-8-sig")
        else:
            assert stem.startswith("ChurchFrontage_Butress"), stem
            wrapper, new_guid, wrapper_text = make_buttress_wrapper(stem, old_guid)
            new_wrappers[wrapper] = (new_guid, wrapper_text)
        assert "Collider:" in wrapper_text, wrapper
        meta_text = model_paths[old_guid].with_name(model_paths[old_guid].name + ".meta").read_text()
        needed[old_guid] = (new_guid, stem, wrapper_ids(wrapper_text, meta_text))
        counts[stem] += 1

    assert sum(counts.values()) == 292, sum(counts.values())
    # Replace source-object references while keeping scene instance IDs and overrides.
    def replace_ref(match: re.Match[str]) -> str:
        fid, old_guid = match.groups()
        if old_guid not in needed:
            return match.group(0)
        new_guid, stem, ids = needed[old_guid]
        if fid == "100100000":
            return f"{{fileID: {fid}, guid: {new_guid}, type: 3}}"
        if fid in ids:
            return f"{{fileID: {ids[fid]}, guid: {new_guid}, type: 3}}"
        # Mesh asset references still point to the original FBX.
        if fid.startswith("430000"):
            return match.group(0)
        raise ValueError(f"No prefab component mapping: {stem} {fid}")

    result = re.sub(r"\{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: 3\}", replace_ref, scene)
    assert result != scene
    assert not any(re.search(r"m_SourcePrefab: \{fileID: 100100000, guid: " + g, result) for g in needed)
    for path, (new_guid, text) in new_wrappers.items():
        path.write_text(text, encoding="utf-8", newline="\n")
        path.with_name(path.name + ".meta").write_text(
            f"fileFormatVersion: 2\nguid: {new_guid}\nPrefabImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n",
            encoding="utf-8", newline="\n",
        )
    SCENE.write_text(result, encoding="utf-8", newline="\n")
    print(f"Replaced {sum(counts.values())} model instances across {len(counts)} models.")
    print(f"Created {len(new_wrappers)} missing collider prefabs.")


if __name__ == "__main__":
    main()
