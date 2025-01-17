using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class Shop : CanvasLayer
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

	public bool isOpen = false;
	public List<ShopItem> shopItems = new();

	private void PrepareShopItems() {
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
	}

	public override void _Ready() {
		Close();
		PrepareShopItems();
		PopulateShopItems();
	}

	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("ui_focus_next")) {
			if (!isOpen) {
				Open();
			} else {
				Close();
			}
		}
	}

	private void PopulateShopItems() {
		for (int i = 0; i < 3; i++) {
			// get nodes
			TextureRect item = (TextureRect) GetNode("Main/SHOP/v/cards/Card" + i + "/Card/v/ItemContainer/Item" + i);
			Label price = (Label) GetNode("Main/SHOP/v/cards/Card" + i + "/Card/v/DescriptionContainer/HBoxContainer/Price" + i);
			Label description = (Label) GetNode("Main/SHOP/v/cards/Card" + i + "/Card/v/DescriptionContainer/HBoxContainer/Description" + i);

			// populate nodes
			item.Texture = (Texture2D) GD.Load(shopItems[i].item);
			price.Text = shopItems[i].price.ToString();
			description.Text = shopItems[i].description;
		}
	}


	private void ChooseShopItem() {

	}

	private void EmitSignalByName(string signal) {
		switch (signal) {
			case "BuyShieldEventHandler":
				EmitSignal(nameof(BuyShieldEventHandler));
				break;
			case "IncreaseSpeedEventHandler":
				EmitSignal(nameof(IncreaseSpeedEventHandler));
				break;
			case "DecreaseDashCooldownEventHandler":
				EmitSignal(nameof(DecreaseDashCooldownEventHandler));
				break;
			case "IncreaseKillzoneTimeEventHandler":
				EmitSignal(nameof(IncreaseKillzoneTimeEventHandler));
				break;
			case "IncreaseOrbDropsEventHandler":
				EmitSignal(nameof(IncreaseOrbDropsEventHandler));
				break;
			default:
				GD.Print("Signal not found: " + signal);
				break;
		}
	}

	private void Open() {
		Visible = true;
		isOpen = true;
	}

	private void Close() {
		Visible = false;
		isOpen = false;
	}
}