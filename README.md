# 目标

预期基于该仓库做游戏开发
1，先把et6入门课、进阶课的内容迁进来
2，然后把技能系统迁移进来
3，配置技能，怪物，实现基本的打怪

4，开始下一期的目标



# 开发环境


et8.1，et8.1eui



Net Sdk 7.0 及以上版本，当前安装了7.0和8.0两个sdk
Unity3D 2021.3.29 及以上版本，基于hyleClr支持的版本和熊猫使用的版本,window build support(il2cpp)，当前使用2022.3.17f1
Rider2023 及以上版本，当前使用rider2025
MongoDB 6.x数据库 及以上版本,课程里说测试过4到7都可用，课程中实际用的是7,作为network service 安装，自己当前用的还是5
Studio 3T数据库可视化软件
vs2022(Rider好一点)
visual studio installer 安装游戏开发环境
（net桌面开发，c++桌面开发，通用window平台开发，c++移动开发，unity游戏开发，c++游戏开发）
rider支持ai插件



# 编译和启动


设置代码编辑器为rider
勾选
Embedded packages
Locai packages

点击Regenerate prolect fies



然后项目中右键打开c# project，再关闭




回到工程目录中打开et.sln,构建编译share工程，再构建编译unity工程
确保无报错

配置好resources/global config中以demo运行，而不是帧同步demo（帧同步demo也行，只不过这不是示例demo，要去看帧同步的代码）


然后回到unity中运行















