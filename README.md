# 标准架构

1. 网络：LiteNetlib + MemoryPack 改造;
2. UI：uGUI + UIManager; 👉改👉 UIToolkit;
3. 消息总线改造：vContainer + MessagePipe + UniRx + UniTask;
4. 使用微服务的服务器；

# AI分析代码

1. 已经写了Editor脚本 /Editor/CodeBundler.cs
2. 打包所有脚本：
```
cd D:\GitHub\UnitySwarm\Client
Get-ChildItem -Path ".\Assets\" -Filter *.cs -Recurse | ForEach-Object { "// File: $($_.FullName)`n" + (Get-Content $_.FullName -Raw) + "`n`n" } | Out-File "ProjectCode.txt" -Encoding utf8
```
3. 打包收订指定脚本
\UnitySwarm\Client\ai_config.txt
```
cd D:\GitHub\UnitySwarm\Client
$list = Get-Content "ai_config.txt" | Where-Object { $_ -and -not $_.StartsWith("#") }; $result = ""; foreach ($f in $list) { $p = Get-ChildItem -Path ".\Assets\" -Filter $f.Trim() -Recurse | Select-Object -First 1; if ($p) { $result += "// File: $($p.FullName)`n" + (Get-Content $p.FullName -Raw) + "`n`n" } }; $result | Out-File "Code_For_AI.txt" -Encoding utf8
```
