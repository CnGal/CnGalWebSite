"""检查公开插件及 ZIP；只使用标准库，不依赖本地测试工程或 Git 暂存区。"""
import argparse
import ast
import hashlib
import json
import re
from pathlib import Path
from zipfile import ZipFile
from package import ROOT, PLUGIN, source_files


def check(output, repository=None):
    source = ROOT / "plugins" / PLUGIN
    files = {path.relative_to(source).as_posix(): path.read_bytes() for path in source_files(source)}
    assert {"main.py", "metadata.yaml", "_conf_schema.json", "README.md", "LICENSE"} <= files.keys()
    assert "core/assets.json" not in files, "素材清单应放在插件数据目录"
    metadata = files["metadata.yaml"].decode("utf-8")
    version = re.search(r"^version:\s*v(\d+\.\d+\.\d+)\s*$", metadata, re.MULTILINE).group(1)
    if repository:
        assert f"repo: https://github.com/{repository}/tree/astrbot-plugin" in metadata.splitlines()
    assert "astrbot_version: '>=4.29.3'" in metadata.splitlines()
    assert "support_platforms: [aiocqhttp]" in metadata.splitlines()
    registration = next(node for node in ast.walk(ast.parse(files["main.py"]))
                        if isinstance(node, ast.Call) and isinstance(node.func, ast.Name) and node.func.id == "register")
    assert ast.literal_eval(registration.args[0]) == PLUGIN
    assert ast.literal_eval(registration.args[3]) == version
    for name, content in files.items():
        if name.endswith(".py"):
            ast.parse(content, filename=name)
        elif name.endswith(".json"):
            json.loads(content)
    assert files["README.md"].strip()
    assert files["LICENSE"] == (ROOT.parents[1] / "LICENSE").read_bytes()
    artifact = output / f"{PLUGIN}-{version}.zip"
    with ZipFile(artifact) as archive:
        assert len(archive.namelist()) == len(files)
        assert {name: archive.read(name) for name in archive.namelist()} == files
    digest = hashlib.sha256(artifact.read_bytes()).hexdigest()
    assert json.loads((output / "sha256.json").read_text()) == {artifact.name: digest}
    return {"version": version, "files": len(files), "sha256": digest}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, default=ROOT / "dist")
    parser.add_argument("--repository", help="发布仓库 owner/name，用于核对安装地址")
    parser.add_argument("--github-output", type=Path)
    args = parser.parse_args()
    result = check(args.output, args.repository)
    if args.github_output:
        with args.github_output.open("a", encoding="utf-8") as stream:
            stream.write(f"version={result['version']}\n")
    print(json.dumps(result, ensure_ascii=False))


if __name__ == "__main__":
    main()
