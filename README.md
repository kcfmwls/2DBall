如果只需要碰撞算法的话可以下载main分支工程，GameMode分支增加了更多游戏性功能✨
- 砖块具有血量设定
- 玩家初始可发射两枚弹球，每吃到一个金色道具可以增加一枚发射的小球
- 每轮发射结束后，所有砖块会下移一格


=========================================================================


## 📖 基于碰撞预测的2D弹球打砖块
- 纯运动学的完全弹性碰撞，没有用到任何物理系统
- 解析法预测碰撞，而非牛顿法。速度再快也不会穿模
- 随机生成7种预设的多边形，可以自己添加预制体和顶点信息当作新的多边形
- 直接运行ball场景即可，小球停下后，再次点击左键会朝紫色方块发射
