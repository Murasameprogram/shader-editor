#  钢铁雄心4 Shader编辑器
2025年12月3日 <br>
编辑器目前处于立项阶段<br>
预计于2025年12月30号前发V1.0.0测试版<br> 
<br> 

2025年12月7日 <br>
ShaderEditor V0.0.1 测试版发布 <br>
1.目前版本的主窗口大部分UI的触发事件暂未实装 <br>
2.打开主窗口后点击左上角启动即可进入编辑器选择界面 <br>
3.目前只有常量编辑器实装且经过测试，其他编辑器处于开发阶段 <br>
4.常量编辑器使用方法为：打开编辑器后打开 钢铁雄心4 的 constants.fxh 文件，路径：Hearts of Iron IV\gfx\FX\constants.fxh ； 成功载入 constants.fxh 文件后,请载入与常量文件同名的 .expl文件(解释文件) <br>
5.编辑器使用.NET框架开发，使用前请安装 .NET 8.0 (桌面运行时)   下载地址: https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.416/dotnet-sdk-8.0.416-win-x64.exe<br>
<br>

2025年12月11日<br>
ShaderEdito技术栈变更以及bug修复日志<br>
1.ShaderEditor在V0.0.2版本将会改用.NET 10框架<br>
2.为了弥补WPF控件生态在“现代化”和“丰富性”上的瓶颈，后续将会使用Blazor生态弥补这一点<br>
3.从V0.0.2版本开始，软件将会采用.NET 10 + WPF+Blazor web的技术栈<br>
<br>
目前已知的bug(下个版本发布时将会修复)<br>
1.保存文件会改变数据结构导致文件不可用<br>
2.载入常量时有概率会载入不完整，例如：10条常量只载入了4条<br>
<br>
ShaderEditor V0.0.2 预更新内容<br>
1.实装V0.0.1版本未能实装的主界面UI交互<br>
2.实装shader生成器和纹理库管理器<br>
3.使用前需安装.NET 10(桌面运行时)和WebView2(桌面运行时)<br>
4.后续可能会将.NET 10(桌面运行时)和WebView2(桌面运行时)打包进软件安装程序，安装程序运行时会自动检测依赖以自动安装依赖框架<br>
<br>
.NET 10(桌面运行时)下载链接：https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.1/windowsdesktop-runtime-10.0.1-win-x64.exe<br>
WebView(桌面运行时)下载链接：https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/c55f85b8-c7f2-4d7e-849d-cd4bfe16b28a/MicrosoftEdgeWebView2RuntimeInstallerX64.exe<br>
