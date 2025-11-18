# Monobehaviour

这个项目有很多垃圾代码因为各种原因被废弃了，而且没有存档功能，你可以随意更改并用于你的项目
以下是简单的项目概括

名字叫V1、V2的文件夹大概率其中的代码是正在维护的，如EnterMessage.cs，它存于Chat/V1/V1.5中，
它的逻辑基于Chat/V1中的代码，它用于在聊天对话功能中接受“ContactInformation.cs”被点击后传
输的信息，然后查找Resources文件夹中的Json聊天文件以及Icon头像，再实例化消息对象实现“根据J
son脚本发送信息”，有链接A、B文件的功能，ContactMemories.cs可以记住你选择了哪个Contact联系人

    你的Resources聊天Json文件大概是这样的：
      "dialogues": [
        {
          "speaker": "MatsuzakaKumi",  //名字
          "speakerAvatar": "KumiIcon", //立绘或者头像图片名字
          "content": "早上好呀！",     //说话内容
          "eventOnDisplay": "",  //改进中
          "eventOnFinish": "",   //改进中
          "autoPlayNext": false, //改进中，保持false就好
          "textSpeed": 1.0   //注释，V1.5文件夹中的Chat对话系统有流式文字传输，TextSpeed数字越小传输速度越快
        },

Player/V1这里是第一人称游戏可能用到的脚本，有移动脚本、载具控制脚本，但是不建议使用，这些是
垃圾代码，有bug并且放弃维护

    Player/V2里的代码是与建造相关的，目前仅有放置功能

    Player/V1/BagManager里的代码是我正在维护的，它可以自由调节Item的尺寸（如2x1、3x4等）而且有
    材料合成逻辑（这是远古代码了，我最近才开始重新维护）
        {
        inventoryCell.cs用于每个单元格，没有严格封装，通过SetState（状态，InventoryItemComponent.cs）设置
        新状态和新的绑定Item

        ItemDragger管理Item的拖拽事件

        InventoryGrid.cs是远古代码了，我忘得差不多了，它有基础的放置、移除、初始化分割背包单元格的功能

        InventoryItemComponent.cs用于储存Item所有信息

        InventoryItem是一个继承自scriptableObject的类，储存Item固定的数据

        ItemPlacer非常重要，这是我最近才重写的Item放置脚本，有大量调试信息，通过它在背包放置Item并占有背包单元格

        ItemFoundManager可以寻找你背包里的物品并组合成一个字典Dict

        CraftManager非常重要，它承载着主要的物品制作逻辑，可以消耗Item的数量并在背包里放置制作物
        }

    Chat/V1文件夹中的内容可用于图片小说（也就是Galgame那种图片小说），我可能会再次扩展功能

    Chat/V1/V1.5文件夹中的代码可用于WeChat式的文字对话，我正在调试并改进（当前版本可能有不足）

        NPC/V1/AiWayPoint/NPCWander.cs这是一个简单的NPC漫游脚本，NPC会随机寻找目标点并前往，有自动
    检查NPC是否遇到障碍物的逻辑

ObjTreeSystem/TreeLODManager.cs已放弃维护，是曾经的旧项目遗留物，可以在空物体的位置上生成
树网格体和自定义碰撞箱（有bug不建议使用）

Map\V1\Cloude是一个简易的云朵生成器，通过参数实例化预制件图片云朵，云朵会在高空生成并跟随
玩家，目前已放弃维护（功能简单可用）

