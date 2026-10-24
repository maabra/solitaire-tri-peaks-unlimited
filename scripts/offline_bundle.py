"""Build a portable Windows folder from locally installed, matching .NET runtimes.

Normal releases should use dotnet publish --self-contained. This fallback is for
offline machines where NuGet runtime packs cannot be downloaded. Runtime manifests
are merged without changing their assembly metadata; Microsoft notices are retained.
"""
import argparse
import json
import shutil
import subprocess
from pathlib import Path


def bundle(source: Path, output: Path, dotnet: Path):
    source, output, dotnet = source.resolve(), output.resolve(), dotnet.resolve()
    if source == output or source in output.parents or output in source.parents:
        raise ValueError("Source and output folders must be separate siblings")
    if not (source / "MeadowPeaks.exe").is_file():
        raise ValueError("Publish the framework-dependent application first")
    config = json.loads((source / "MeadowPeaks.runtimeconfig.json").read_text("utf-8-sig"))
    frameworks = config["runtimeOptions"].pop("frameworks")
    runtime_versions = {}
    for line in subprocess.check_output([str(dotnet / "dotnet.exe"), "--list-runtimes"], text=True).splitlines():
        name, version, *_ = line.split()
        runtime_versions.setdefault(name, []).append(version)
    chosen = {}
    for framework in frameworks:
        name = framework["name"]
        major_minor = ".".join(framework["version"].split(".")[:2]) + "."
        candidates = [v for v in runtime_versions.get(name, []) if v.startswith(major_minor) and "-" not in v]
        if not candidates:
            raise ValueError(f"Missing local runtime {name} {major_minor}x")
        chosen[name] = max(candidates, key=lambda v: tuple(map(int, v.split("."))))
    if len(set(chosen.values())) != 1:
        raise ValueError("Offline packaging requires matching Core and Desktop runtime patch versions")
    output.mkdir(parents=True, exist_ok=True)
    for item in source.iterdir():
        if item.is_file() and item.suffix in (".exe", ".dll", ".json"):
            shutil.copy2(item, output / item.name)
    appdeps = json.loads((source / "MeadowPeaks.deps.json").read_text("utf-8-sig"))
    app_target = appdeps["targets"][appdeps["runtimeTarget"]["name"]]
    merged = dict(app_target)
    app = merged[next(key for key in merged if key.split("/")[0] == "MeadowPeaks")]
    app.setdefault("dependencies", {})
    for name, version in chosen.items():
        runtime = dotnet / "shared" / name / version
        shutil.copytree(runtime, output, dirs_exist_ok=True)
        manifest = json.loads((runtime / f"{name}.deps.json").read_text("utf-8-sig"))
        target_name = manifest["runtimeTarget"]["name"]
        if not target_name.endswith("/win-x64"):
            raise ValueError("This packaging helper requires x64 Windows runtimes")
        for key, assets in manifest["targets"][target_name].items():
            merged[key] = assets
            library, library_version = key.rsplit("/", 1)
            app["dependencies"][library] = library_version
            appdeps["libraries"][key] = {"type": "runtimepack", "serviceable": False, "sha512": ""}
    appdeps["runtimeTarget"]["name"] = target_name
    appdeps["targets"] = {target_name: merged}
    config["runtimeOptions"]["includedFrameworks"] = [{"name": k, "version": v} for k, v in chosen.items()]
    version = chosen["Microsoft.NETCore.App"]
    shutil.copy2(dotnet / "host" / "fxr" / version / "hostfxr.dll", output / "hostfxr.dll")
    for notice in ("LICENSE.txt", "ThirdPartyNotices.txt"):
        if (dotnet / notice).exists():
            shutil.copy2(dotnet / notice, output / notice)
    (output / "MeadowPeaks.deps.json").write_text(json.dumps(appdeps, indent=2), "utf-8")
    (output / "MeadowPeaks.runtimeconfig.json").write_text(json.dumps(config, indent=2), "utf-8")
    print(f"Portable runtime bundle ready: {output}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--dotnet-root", type=Path, default=Path(shutil.which("dotnet")).parent)
    args = parser.parse_args()
    bundle(args.source, args.output, args.dotnet_root)
