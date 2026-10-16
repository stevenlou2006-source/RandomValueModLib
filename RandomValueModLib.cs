using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RandomValueLib
{
	/// <summary>
	/// 随机化配置
	/// </summary>
	public class RandomizationConfig
	{
		public float MinMultiplier { get; set; } = 0.5f;
		public float MaxMultiplier { get; set; } = 2.0f;
		public bool EnableBlacklist { get; set; } = true;
		public List<string> BlacklistKeywords { get; set; } = new List<string>();
		public bool EnableWhitelist { get; set; } = false;
		public List<string> WhitelistKeywords { get; set; } = new List<string>();
		public bool ExcludeHighValueItems { get; set; } = false;
		public float HighValueThreshold { get; set; } = 50000f;
		public bool RandomizeCashBags { get; set; } = false;
	}

	/// <summary>
	/// 核心随机价格服务
	/// </summary>
	public static class RandomValueService
	{
		private static FieldInfo dollarValueCurrentField;
		private static FieldInfo dollarValueOriginalField;

		static RandomValueService()
		{
			var valuableType = typeof(ValuableObject);
			dollarValueCurrentField = valuableType.GetField("dollarValueCurrent",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			dollarValueOriginalField = valuableType.GetField("dollarValueOriginal",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		}

		/// <summary>
		/// 获取物品当前价格（反射）
		/// </summary>
		public static float GetItemPrice(ValuableObject vo)
		{
			if (dollarValueCurrentField == null) return 0f;
			return (float)dollarValueCurrentField.GetValue(vo);
		}

		/// <summary>
		/// 设置物品价格（反射）
		/// </summary>
		public static void SetItemPrice(ValuableObject vo, float newPrice)
		{
			dollarValueCurrentField?.SetValue(vo, newPrice);
			dollarValueOriginalField?.SetValue(vo, newPrice);
		}

		/// <summary>
		/// 纯随机价格计算（不修改物品）
		/// </summary>
		public static float GetRandomPrice(int instanceID, float originalPrice, RandomizationConfig config, int? extraSeed = null)
		{
			float min = Math.Max(0.1f, config.MinMultiplier);
			float max = Math.Max(min + 0.1f, config.MaxMultiplier);

			int seed = instanceID;
			if (extraSeed.HasValue)
				seed ^= extraSeed.Value;

			var rand = new System.Random(seed);
			float multiplier = (float)(rand.NextDouble() * (max - min) + min);

			float newPrice = originalPrice;
			if (originalPrice > 0)
			{
				newPrice = originalPrice * multiplier;
				newPrice = Mathf.Clamp(newPrice, 1f, 1000000f);
			}
			return newPrice;
		}

		/// <summary>
		/// 检查物品是否应被随机化（黑白名单、现金袋等）
		/// </summary>
		public static bool ShouldRandomize(GameObject obj, RandomizationConfig config)
		{
			string name = obj.name.ToLower();

			if (name.Contains("surplus") && !config.RandomizeCashBags)
				return false;

			if (config.ExcludeHighValueItems)
			{
				var vo = obj.GetComponent<ValuableObject>();
				if (vo != null && GetItemPrice(vo) >= config.HighValueThreshold)
					return false;
			}

			if (config.EnableBlacklist && config.BlacklistKeywords.Any(k => name.Contains(k)))
				return false;

			if (config.EnableWhitelist && !config.WhitelistKeywords.Any(k => name.Contains(k)))
				return false;

			return true;
		}

		/// <summary>
		/// 完整应用随机价格（含规则检查和价格修改）
		/// </summary>
		public static float ApplyRandomPrice(ValuableObject vo, RandomizationConfig config, int? extraSeed = null)
		{
			if (vo == null) return 0f;

			int instanceID = vo.gameObject.GetInstanceID();
			float original = GetItemPrice(vo);
			if (original <= 0) return original;

			if (!ShouldRandomize(vo.gameObject, config))
				return original;

			float newPrice = GetRandomPrice(instanceID, original, config, extraSeed);
			SetItemPrice(vo, newPrice);
			return newPrice;
		}
	}

	/// <summary>
	/// ValuableObject 扩展方法（可选）
	/// </summary>
	public static class ValuableObjectExtensions
	{
		public static float ApplyRandomPrice(this ValuableObject vo, RandomizationConfig config, int? extraSeed = null)
		{
			return RandomValueService.ApplyRandomPrice(vo, config, extraSeed);
		}
	}
}