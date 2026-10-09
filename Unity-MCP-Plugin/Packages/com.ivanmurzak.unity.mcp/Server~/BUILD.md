# Bundled GameDev-MCP-Server (Changefied build)

This folder holds the MCP server that the plugin installs into `Library/mcp-server/<rid>/` instead of
downloading the official release. Unity ignores folders ending in `~`, so nothing here is imported.

## Why it exists

The official server gives the editor 10 s to answer a tool call. When that runs out it sends the same call
again, up to 10 times. The editor never drops the earlier copies: it runs every one of them on its main thread.
One slow call (a large graph edit, a script that takes 11 s) therefore turned into 11 queued executions, and
every later call waited behind the pile. On 2026-10-09 that cost about 7 minutes in one session, and
`script-execute` calls were found to have run 2 to 7 times each. Across two weeks of server logs a resend never
turned a failure into a genuine success.

The patch in `no-resend.patch` changes that:

- a call the editor received and did not answer in time fails once with a timeout; it is not re-sent;
- a call that errors on the editor side fails once; it is not re-sent;
- only "no editor connected" is still waited out, for up to 30 s, so a call made during a domain reload lands.

The plugin's default timeout is raised to 120 s alongside it (`UnityConnectionConfig.DefaultTimeoutMs`), so
ordinary slow calls finish instead of failing.

## What is here

| Path | What |
|---|---|
| `win-x64/` | The publish output the plugin copies, unchanged except that `.pdb` and `appsettings.Development.json` are left out. |
| `win-x64/gamedev-mcp-server.exe.sha256` | SHA-256 of the exe. The plugin refuses to install an exe that does not match. |
| `no-resend.patch` | The change to `MCP-Plugin-dotnet`, against tag `7.5.0`. |

Only Windows x64 is bundled. On any other platform the plugin reports that no bundled server exists.

## Rebuilding

Versions must match what server `9.2.2` was built from: GameDev-MCP-Server `v9.2.2`, MCP-Plugin-dotnet `7.5.0`,
ReflectorNet `5.3.2`. The server project has a `UseLocalMcpPlugin` switch that builds against sibling checkouts.

```sh
mkdir mcp-build && cd mcp-build
git clone --depth 1 --branch v9.2.2 https://github.com/IvanMurzak/GameDev-MCP-Server.git
git clone --depth 1 --branch 7.5.0  https://github.com/IvanMurzak/MCP-Plugin-dotnet.git
git clone --depth 1 --branch 5.3.2  https://github.com/IvanMurzak/ReflectorNet.git
git -C MCP-Plugin-dotnet apply <this folder>/no-resend.patch

cd GameDev-MCP-Server
dotnet publish com.IvanMurzak.GameDev.MCP.Server.csproj -c Release -r win-x64 --self-contained true \
    -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:UseLocalMcpPlugin=true \
    -o ../publish/win-x64
```

Needs the .NET 9 SDK. `EnableCompressionInSingleFile` brings the exe from 103 MB to about 50 MB, under GitHub's
100 MB file limit, so it is committed as an ordinary file rather than through Git LFS.

Then copy the publish output into `win-x64/` (without `*.pdb` and `appsettings.Development.json`), write the
new hash with `sha256sum gamedev-mcp-server.exe | cut -d' ' -f1 > gamedev-mcp-server.exe.sha256`, and **bump
`McpServerManager.BundledServerBuild`** (`9.2.2-changefied.1` -> `.2`). The bump is what makes every existing
`Library/mcp-server` replace its copy; without it, editors keep running the old exe.

## Moving to a newer upstream plugin

Merge the upstream tag into this branch, then check `McpServerManager.ServerVersion`. If it changed, rebuild the
server from that version (and the MCP-Plugin-dotnet / ReflectorNet versions its csproj pins), re-apply the patch,
and bump `BundledServerBuild`. Check first whether upstream has stopped re-sending received calls on its own; if
it has, this whole folder can go.
