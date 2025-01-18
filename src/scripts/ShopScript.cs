using Godot;
using System;
using System.Collections.Generic;

public partial class ShopScript : Control
{
	[Signal] public delegate void BuyShieldEventHandler();
	[Signal] public delegate void IncreaseSpeedEventHandler(float percentage);
	[Signal] public delegate void DecreaseDashCooldownEventHandler(float percentage);
	[Signal] public delegate void IncreaseKillzoneTimeEventHandler(float percentage);
	[Signal] public delegate void IncreaseOrbDropsEventHandler();

	[Export] public float speedIncreasePercent = 0.05f;
	[Export] public float dashCooldownDecreasePercent = 0.10f;
	[Export] public float killZoneIncreasePercent = 0.15f;
	[Export] public float fadeTime = 0.2f;

	public struct ShopItem {
		public string id;
		public string item;
		public int price;
		public string description;
		public string signal;
	}

	Shop canvasItem;
	Globals globals;
	public bool isOpen = false;

	/**
	 * Shop items available
	 */
	public List<ShopItem> shopItems = new();

	/**
	 * Active shop items, the ones seen in the shop (limit: 3)
	 */
	public ShopItem[] activeShopItems = new ShopItem[3];

	public void PrepareShopItems() {
		shopItems.Add
        (
            new ShopItem
            {
				id = "shield",
                item = "res://assets/menu/shop_items/shield.png",
                price = 60,
                description = " ~ +1 Shield",
                signal = "BuyShieldEventHandler"
            }
		);
		shopItems.Add
        (
            new ShopItem
            {
				id = "orbs",
                item = "res://assets/menu/shop_items/purple_orb.png",
                price = 75,
                description = " ~ +1 Orb / Kill",
                signal = "IncreaseOrbDropsEventHandler"
            }
		);
		shopItems.Add
        (
            new ShopItem
            {
				id = "dash",
                item = "res://assets/menu/shop_items/wind_dash.png",
                price = 30,
                description = " ~ +10% Dash",
                signal = "DecreaseDashCooldownEventHandler"
            }
		);
		shopItems.Add
        (
            new ShopItem
            {
				id = "killzone",
                item = "res://assets/menu/shop_items/sword_attack.png",
                price = 100,
                description = " ~ +15% Killzone",
                signal = "IncreaseKillzoneTimeEventHandler"
            }
		);
		shopItems.Add
        (
            new ShopItem
            {
				id = "speed",
                item = "res://assets/menu/shop_items/hermes_boots_speed.png",
                price = 20,
                description = " ~ +5% Speed",
                signal = "IncreaseSpeedEventHandler"
            }
		);
	}

	public override void _Ready() {
		globals = (Globals) GetNode("/root/Globals");
		canvasItem = (Shop) GetParent();
		PrepareShopItems();
		PopulateActiveShopItems();
		PopulateShopItems();
	}

	public override void _Process(double delta) {
		HandleInput();
	}

	/*
	Handles input for shop
	*/
	private void HandleInput() {
		// buy items
		if (Input.IsActionJustPressed("buy_one")) {
			BuyItem(0);
		}
		if (Input.IsActionJustPressed("buy_two")) {
			BuyItem(1);
		}
		if (Input.IsActionJustPressed("buy_three")) {
			BuyItem(2);
		}
	}

	private void BuyItem(int index) {
		ShopItem item = activeShopItems[index];
		if (globals.Experience >= item.price) {
			// buy successful
			globals.AddExperience(-item.price);
			EmitSignalByName(item.signal);
			BuySuccessVisuals(index);
		} else {
			// fail to buy
			BuyFailVisuals(index);
		}
	}

	private void BuyRepopulate(int index) {
		activeShopItems[index] = ChooseRandomShopItem();
		PopulateShopItems();
	}

	private void BuySuccessVisuals(int index) {
		// fade in and out on buy
		Panel card = (Panel) GetNode("SHOP/v/cards/Card" + index + "/Card");
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(card, "modulate:a", 0, fadeTime);
		tween.TweenCallback(Callable.From(() => {BuyRepopulate(index);}));
		tween.TweenInterval(0.1f);
		tween.TweenProperty(card, "modulate:a", 1, fadeTime);
	}

	private void BuyFailVisuals(int index) {
		// flash red on fail to buy
		Panel card = (Panel) GetNode("SHOP/v/cards/Card" + index + "/Card");
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(card, "modulate", new Color("e83b3b"), fadeTime / 2);
		tween.TweenProperty(card, "modulate", new Color(1, 1, 1, 1), fadeTime / 2);
	}

	private void PopulateShopItems() {
		for (int i = 0; i < 3; i++) {
			// get nodes
			TextureRect item = (TextureRect) GetNode("SHOP/v/cards/Card" + i + "/Card/v/ItemContainer/Item" + i);
			Label price = (Label) GetNode("SHOP/v/cards/Card" + i + "/Card/v/DescriptionContainer/HBoxContainer/Price" + i);
			Label description = (Label) GetNode("SHOP/v/cards/Card" + i + "/Card/v/DescriptionContainer/HBoxContainer/Description" + i);

			// populate nodes
			item.Texture = (Texture2D) GD.Load(activeShopItems[i].item);
			price.Text = activeShopItems[i].price.ToString();
			description.Text = activeShopItems[i].description;
		}
	}


	private void PopulateActiveShopItems() {
		for (int i = 0; i < 3; i++) {
			activeShopItems[i] = ChooseRandomShopItem();
		}
	}

	private ShopItem ChooseRandomShopItem() {
		Random random = new Random();
		return shopItems[random.Next(0, shopItems.Count)];
	}

	/*
	Helper function to allow for emission of signals through signal name
	*/
	private void EmitSignalByName(string signal) {
		switch (signal) {
			case "BuyShieldEventHandler":
				EmitSignal(nameof(BuyShield));
				break;
			case "IncreaseSpeedEventHandler":
				EmitSignal(nameof(IncreaseSpeed), speedIncreasePercent);
				break;
			case "DecreaseDashCooldownEventHandler":
				EmitSignal(nameof(DecreaseDashCooldown), dashCooldownDecreasePercent);
				break;
			case "IncreaseKillzoneTimeEventHandler":
				EmitSignal(nameof(IncreaseKillzoneTime), killZoneIncreasePercent);
				break;
			case "IncreaseOrbDropsEventHandler":
				globals.ExperienceAddOns++;
				break;
			default:
				GD.Print("Signal not found: " + signal);
				break;
		}
	}
}
