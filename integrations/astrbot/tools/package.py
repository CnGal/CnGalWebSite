"""打包交互插件的公开源码，人设和素材由部署时另行导入。"""
import argparse
import hashlib
import json
import re
import shutil
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

ROOT = Path(__file__).resolve().parents[1]
PLUGIN = "astrbot_plugin_cngal_interaction"


def source_files(source):
    """排除各层级的环境和缓存文件，拒绝混入测试或私有资料。"""
    files = []
    for path in sorted(source.rglob("*")):
        relative = path.relative_to(source)
        if not path.is_file() or set(relative.parts) & {".venv", ".pytest_cache", "__pycache__"} or path.suffix == ".pyc":
            continue
        if set(relative.parts) & {"private", "tests"} or relative.parts[0] not in {
                "main.py", "metadata.yaml", "_conf_schema.json", "README.md", "LICENSE", "core"}:
            raise ValueError(f"插件目录包含非公开源码文件：{relative.as_posix()}")
        files.append(path)
    return files


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, default=ROOT / "dist")
    args = parser.parse_args()
    output = args.output.resolve()
    if (output == ROOT or output in ROOT.parents or any(
            output.is_relative_to(ROOT / name)
            for name in ("plugins", "tools", "local", "tests", "deployment", "contracts", "private"))):
        raise ValueError("请使用独立的输出目录，不能写入源码目录")
    output.mkdir(parents=True, exist_ok=True)
    source = ROOT / "plugins" / PLUGIN
    target = output / "plugins" / PLUGIN
    version = re.search(r"^version:\s*v?(\d+\.\d+\.\d+)\s*$",
                        (source / "metadata.yaml").read_text(encoding="utf-8"), re.MULTILINE)
    if version is None:
        raise ValueError(f"插件缺少版本号：{PLUGIN}")
    files = source_files(source)
    if target.exists():
        if target.resolve() == output or not target.resolve().is_relative_to(output):
            raise ValueError("待清理目录不在打包输出目录内")
        shutil.rmtree(target)
    for file in files:
        destination = target / file.relative_to(source)
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(file, destination)
    artifact = output / f"{PLUGIN}-{version[1]}.zip"
    with ZipFile(artifact, "w", ZIP_DEFLATED) as z:
        for file in files:
            relative = file.relative_to(source)
            z.write(target / relative, relative.as_posix())
    manifest = {artifact.name: hashlib.sha256(artifact.read_bytes()).hexdigest()}
    (output / "sha256.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
