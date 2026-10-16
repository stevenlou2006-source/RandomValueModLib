## 💬 交流反馈
QQ 群：**573485890**，欢迎进群讨论、反馈问题。

---

# RandomValueLib


**版本：1.0.5 | 许可证：MIT**

## 📖 概述
核心随机价格库，为《REPO》模组提供统一的贵重物品价格随机化逻辑。

## ✨ 功能
- 确定性随机算法（基于物品 ID 和可选额外种子）
- 可配置倍率范围、黑白名单、现金袋保护、高价物品排除
- 提供纯计算和直接修改两种方式
- 反射读写价格字段，兼容公共化与非公共化环境

## 📦 安装
将 `RandomValueLib.dll` 放入 `BepInEx/plugins` 文件夹（作为其他模组的依赖自动安装，一般无需手动操作）。

## 🔌 使用示例
```csharp
using RandomValueLib;

var config = new RandomizationConfig
{
    MinMultiplier = 0.5f,
    MaxMultiplier = 2.0f,
    // 根据需要设置其他规则
};

// 方式一：直接应用随机价格
float newPrice = RandomValueService.ApplyRandomPrice(valuableObject, config);

// 方式二：先判断是否随机化
if (RandomValueService.ShouldRandomize(obj, config))
{
    float price = RandomValueService.GetRandomPrice(instanceID, originalPrice, config);
    RandomValueService.SetItemPrice(vo, price);
}

---

## 许可证

本项目以 **GNU General Public License v3.0** 发布。

Copyright (C) 2026 cn_xc

完整条款见仓库根目录 [LICENSE](LICENSE)。
