using Godot;
using System;
using System.Collections.Generic;

public partial class ShopScript : Control
{
	[Signal] public delegate void BuyShieldEventHandler();
	[Signal] public delegate void IncreaseSpeedEventHandler();
	[Signal] public delegate void DecreaseDashCooldownEventHandler();
	[Signal] public delegate void IncreaseKillzoneTimeEventHandler();
	[Signal] public delegate void IncreaseOrbDropsEventHandler();

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
                price = 100,
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
                price = 200,
                description = " ~ +1 Orb / Kill",
                signal = "IncreaseOrbDropEventHandler"
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
			activeShopItems[index] = ChooseRandomShopItem();
			PopulateShopItems();
		} else {
			// fail to buy
			GD.Print("Not enough experience to buy " + item.id);
		}
		
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
				EmitSignal(nameof(IncreaseSpeed));
				break;
			case "DecreaseDashCooldownEventHandler":
				EmitSignal(nameof(DecreaseDashCooldown));
				break;
			case "IncreaseKillzoneTimeEventHandler":
				EmitSignal(nameof(IncreaseKillzoneTime));
				break;
			case "IncreaseOrbDropsEventHandler":
				EmitSignal(nameof(IncreaseOrbDrops));
				break;
			default:
				GD.Print("Signal not found: " + signal);
				break;
		}
	}
}
