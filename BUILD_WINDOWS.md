Windows build: how to get a runnable EXE

What I added
- A GitHub Actions workflow at `.github/workflows/windows-build.yml` that builds the solution on `windows-latest` and uploads the Release output as an artifact.

How to get the EXE (two options)

1) Trigger via push (easy)
- Commit and push these changes to `main` (or open a PR and merge). The workflow runs automatically on push.

2) Trigger manually from GitHub (no push needed)
- Go to the repository on GitHub → Actions → "Build Valex (Windows)" → Run workflow → Select branch `main` → Run.

Download the build artifact
- After the workflow completes, open the workflow run page → Artifacts → download `Valex-windows-build` → extract. The EXE will be in the `bin\Release\net481\` folder inside the artifact (or `Valex\Valex\bin\Release\net481\`).

Prerequisites on the target machine
- Install Microsoft .NET Framework 4.8 (or the Developer Pack/runtime) if not present.
- Install Microsoft Edge WebView2 Runtime (Evergreen) — required by `Microsoft.Web.WebView2`.

Notes
- The workflow builds `Release` / `x64`. If the build fails due to missing DLLs referenced via absolute HintPaths, you may need to install the missing NuGet packages or provide those DLLs in the repository (I left references as-is). If you want, I can attempt to fix references to rely entirely on NuGet packages.
