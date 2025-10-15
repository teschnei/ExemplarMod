using System;
using System.Collections.Generic;
using System.Linq;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Controls;
using Dawnsbury.Display.DragAndDrop;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Classes.Exemplar.Ikons;
using Dawnsbury.Mods.Classes.Exemplar.RegisteredComponents;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Classes.Exemplar;

[HarmonyPatch(typeof(CombatActionExecution), nameof(CombatActionExecution.DetermineGuaranteedRollNumber))]
public static class UnfailingBowPatch
{
    static void Postfix(CombatAction combatAction, ref int __result)
    {
        var unfailing = combatAction.Owner.FindQEffect(Ikon.IkonLUT[ExemplarFeats.UnfailingBow].EmpoweredQEffectId);
        if (unfailing != null && unfailing.Tag != null)
        {
            __result = (int)unfailing.Tag;
        }
    }
}

[HarmonyPatch(typeof(Tile), nameof(Tile.CountsAsNonignoredDifficultTerrainFor))]
public static class BornOfTheBonesOfTheEarthPatch
{
    static void Postfix(Tile __instance, Creature who, ref bool __result)
    {
        var born = __instance.TileQEffects.Where(q => q.TileQEffectId == ExemplarTileQEffects.BornOfTheBonesOfTheEarthTerrain).FirstOrDefault();
        if (__result == true && (who.PersistentCharacterSheet?.Calculated.HasFeat(ExemplarFeats.BornOfTheBonesOfTheEarth) ?? false))
        {
            __result = false;
        }
    }
}

static class InventoryState
{
    public static Item? ikonExpanded = null;
    public static Item? lastMouseOver = null;
}

[HarmonyPatch(typeof(CharacterInventoryControl), nameof(CharacterInventoryControl.Draw), typeof(CharacterSheet), typeof(Rectangle), typeof(int), typeof(bool))]
static class IkonSelectionPatch
{
    class Buttons
    {
        public Rectangle bg;
        public List<Rectangle> buttons = new();
        public Rectangle cancel;
    }

    static Buttons? CalcMenu(Rectangle rect, int buttonCount)
    {
        if (buttonCount == 0)
        {
            return null;
        }
        var inset = 10;
        var buttonHeight = 120;
        var frameHeight = Primitives.Unscale((buttonCount + 1) * (buttonHeight + inset) + (inset * 2) + 100, Primitives.ScaleY);
        Rectangle bg = new Rectangle(rect.X, (rect.Y + rect.Height - frameHeight) / 2, rect.Width, frameHeight);
        var bgScaled = Primitives.Scale(bg);

        var y = bgScaled.Top + 100 + inset;
        List<Rectangle> buttons = new();
        for (int i = 0; i < buttonCount; ++i)
        {
            Rectangle rectangle7 = new Rectangle(bgScaled.X + inset, y, bgScaled.Width - (inset * 2), buttonHeight);
            y += buttonHeight + inset;
            buttons.Add(rectangle7);
        }
        Rectangle closeButton = new Rectangle(bgScaled.X + inset, bgScaled.Bottom - buttonHeight - inset, bgScaled.Width - (inset * 2), buttonHeight);
        return new Buttons()
        {
            bg = bg,
            buttons = buttons,
            cancel = closeButton
        };
    }

    static void Prefix(CharacterSheet sheet, Rectangle rect, int atLevel, out Buttons? __state)
    {
        if (InventoryState.ikonExpanded != null)
        {
            Item item = InventoryState.ikonExpanded;

            var ikons = Ikons.Ikon.IkonLUT.Values.Where(i => sheet?.Calculated.AllFeatGrants.Any(fg => fg.GrantedFeat == i.IkonFeat && fg.AtLevel <= (sheet.IsCampaignCharacter ? sheet.MaximumLevel : sheet.EditingInventoryAtLevel) && i.Equippable && i.ValidItem?.Invoke(item) == null) ?? false);

            __state = CalcMenu(rect, ikons.Count());
            if (__state != null)
            {
                if (!Root.IsMouseOver(__state.bg) && Root.WasMouseLeftClick)
                {
                    InventoryState.ikonExpanded = null;
                    Root.ConsumeLeftClick();
                }
                if (Root.WasMouseRightClick)
                {
                    InventoryState.ikonExpanded = null;
                    Root.ConsumeRightClick();
                }
            }
        }
        else
        {
            __state = null;
        }
    }
    static void Postfix(CharacterSheet sheet, Rectangle rect, int atLevel, bool avoidDrawingFace, Buttons? __state)
    {
        var allItems = sheet.Inventory.Backpack.Concat([sheet.Inventory.LeftHand, sheet.Inventory.RightHand, sheet.Inventory.Armor]);
        var allIkons = Ikons.Ikon.IkonLUT.Values.Where(i => i.Equippable && (sheet?.Calculated.AllFeatGrants.Any(fg => fg.GrantedFeat == i.IkonFeat && fg.AtLevel <= (sheet.IsCampaignCharacter ? sheet.MaximumLevel : sheet.EditingInventoryAtLevel)) ?? false));
        if (InventoryState.ikonExpanded != null && __state != null)
        {
            Item item = InventoryState.ikonExpanded;

            var ikons = allIkons.Where(i => i.ValidItem?.Invoke(item) == null);
            var ikonMods = item.ItemModifications.Where(mod => mod.Kind == ItemModificationKind.CustomPermanent && ((string?)mod.Tag ?? "").StartsWith("ikon"));
            var selectedIkons = ikons.Where(ikon => (ikonMods.Select(mod => mod.Tag as string).Any(mt => mt?.Contains(ikon.IkonFeat.FeatName.ToStringOrTechnical()) ?? false)));

            Primitives.DrawAndFillRectangle(__state.bg, ColorScheme.Instance.MenuBackgroundColorLighter, Color.Black);
            Writer.DrawString("Ikon Management", new Rectangle(__state.bg.Left, __state.bg.Top, __state.bg.Width, 70), alignment: Writer.TextAlignment.Middle);

            foreach (var (ikon, button) in ikons.Zip(__state.buttons))
            {
                var selected = selectedIkons?.Contains(ikon) ?? false;
                bool mouseOver = Root.IsMouseOverNative(button);
                string? disabledBy = Ikons.Ikon.IkonExpansionReqLUT.TryGetValue(ikon.IkonFeat.FeatName, out var d) ? d
                    .Where(kvp => sheet?.Calculated.AllFeatGrants.Any(fg => fg.GrantedFeat.FeatName == kvp.Key && fg.AtLevel <= (atLevel >= 1 ? atLevel : sheet.MaximumLevel)) ?? false)
                    .Select(kvp => kvp.Value(item)).FirstOrDefault() : null;
                UI.DrawUIButton(Primitives.Unscale(button), (rect) =>
                {
                    if (selected && mouseOver)
                    {
                        Primitives.FillRectangle(rect, Color.LightGreen);
                    }
                    Writer.DrawString(disabledBy == null ? ikon.IkonFeat.Name : $"{ikon.IkonFeat.Name} (disabled by {disabledBy})", rect, alignment: Writer.TextAlignment.Middle);
                }, () =>
                {
                    if (selectedIkons?.Contains(ikon) ?? false)
                    {
                        item.ItemModifications.RemoveAll(mod => mod.Kind == ItemModificationKind.CustomPermanent && (string?)mod.Tag == ikon.ModString);
                    }
                    else
                    {
                        foreach (var i in allItems)
                        {
                            i?.ItemModifications.RemoveAll(mod => mod.Kind == ItemModificationKind.CustomPermanent && (string?)mod.Tag == ikon.ModString);
                        }
                        item.WithModification(ikon.IkonModification);
                    }
                }, disabledBy == null, ikon.IkonFeat.FullTextDescription, selected ? Color.Green : null);

                if (mouseOver)
                {
                    DragAndDrop.MouseOverDraggableItem = null;
                }
            }
            UI.DrawUIButton(Primitives.Unscale(__state.cancel), "Close", () =>
            {
                DragAndDrop.MouseOverDraggableItem = null;
                InventoryState.ikonExpanded = null;
            });
        }

        //Warn if there are any unset ikons
        var equippedIkons = allItems.Where(i => i != null).SelectMany(i => i!.ItemModifications.Where(mod => mod.Kind == ItemModificationKind.CustomPermanent && (((string?)mod.Tag)?.StartsWith("ikon") ?? false))).Select(mod => (string?)mod.Tag ?? "");
        var availableIkons = allIkons.Select(ikon => ikon.ModString);
        if (equippedIkons.Intersect(availableIkons).Count() != availableIkons.Count())
        {
            Rectangle warning = new Rectangle(rect.Right - 80, rect.Top + (avoidDrawingFace ? 0 : 200), 80, 90);
            Writer.DrawString("{icon:RedWarning}", warning, Color.Red, null, Writer.TextAlignment.Right);
            if (Root.IsMouseOver(warning))
            {
                Tooltip.DrawTooltipAround(Primitives.Scale(warning), "You have unassigned ikons.");
            }
        }
    }
}

[HarmonyPatch(typeof(CharacterInventoryControl), ("DrawSlot"))]
static class IkonSelectionPatch2
{
    static void Postfix(Rectangle rectangle, InventoryItemSlot itemSlot)
    {
        var sheet = itemSlot.CharacterSheet;
        if (sheet != null)
        {
            if (itemSlot.Item != null)
            {
                var ikons = Ikons.Ikon.IkonLUT.Values.Where(i =>
                        sheet?.Calculated.AllFeatGrants.Any(fg => fg.GrantedFeat == i.IkonFeat && fg.AtLevel <= (sheet.IsCampaignCharacter ? sheet.MaximumLevel : sheet.EditingInventoryAtLevel) && i.Equippable && i.ValidItem?.Invoke(itemSlot.Item) == null) ?? false);
                if (ikons.Count() > 0)
                {
                    int height = rectangle.Height / 4;
                    int width = rectangle.Width;
                    Rectangle rectangle6 = new Rectangle(rectangle.X, rectangle.Top - height, width, height);
                    bool mouseOverItem = Root.IsMouseOverNative(rectangle);
                    bool mouseOverButton = Root.IsMouseOverNative(rectangle6);
                    if (mouseOverButton)
                    {
                        DragAndDrop.MouseOverDraggableItem = null;
                    }
                    else if (InventoryState.lastMouseOver == itemSlot.Item)
                    {
                        InventoryState.lastMouseOver = null;
                    }
                    if (mouseOverItem && InventoryState.lastMouseOver == null)
                    {
                        InventoryState.lastMouseOver = itemSlot.Item;
                    }
                    if ((mouseOverItem || mouseOverButton) && InventoryState.lastMouseOver == itemSlot.Item)
                    {
                        Primitives.DrawAndFillRectangleNative(rectangle6, mouseOverButton ? ColorScheme.Instance.ButtonMouseOver : ColorScheme.Instance.ButtonBackground, Color.Black);
                        Writer.DrawStringNative("Manage Ikons", rectangle6, Color.Black, null, Writer.TextAlignment.Middle);
                    }
                    if (mouseOverButton && Root.WasMouseLeftClick)
                    {
                        InventoryState.ikonExpanded = itemSlot.Item;
                        Root.ConsumeLeftClick();
                    }
                }
            }
        }
    }
}

