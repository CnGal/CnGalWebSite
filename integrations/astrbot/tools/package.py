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


def remove_owned_directory(target, output):
    if target.exists():
        if target.resolve() == output or not target.resolve().is_relative_to(output):
            raise ValueError("Output escaped package directory")
        shutil.rmtree(target)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, default=ROOT / "dist")
    args = parser.parse_args()
    output = args.output.resolve()
    if (output == ROOT or output in ROOT.parents or any(
            output.is_relative_to(ROOT / name)
            for name in ("plugins", "tools", "local", "tests", "deployment", "contracts", "private"))):
        raise ValueError("Use a dedicated output folder, not a source directory")
    output.mkdir(parents=True, exist_ok=True)
    manifest = {}
    source = ROOT / "plugins" / PLUGIN
    target = output / "plugins" / PLUGIN
    version = re.search(r"^version:\s*v?(\d+\.\d+\.\d+)\s*$",
                        (source / "metadata.yaml").read_text(encoding="utf-8"), re.MULTILINE)
    if version is None:
        raise ValueError(f"Missing package version: {PLUGIN}")
    # 插件目录即完整安装源码；private/ 位于目录外，不参与复制。
    remove_owned_directory(target, output)
    shutil.copytree(source, target, ignore=shutil.ignore_patterns("__pycache__", "*.pyc"))
    artifact = output / f"{PLUGIN}-{version[1]}.zip"
    with ZipFile(artifact, "w", ZIP_DEFLATED) as z:
        for file in sorted(target.rglob("*")):
            if file.is_file():
                z.write(file, str(file.relative_to(target)).replace("\\", "/"))
    manifest[artifact.name] = hashlib.sha256(artifact.read_bytes()).hexdigest()
    (output / "sha256.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
