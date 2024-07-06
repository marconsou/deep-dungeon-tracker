using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DeepDungeonTracker;

public unsafe static partial class NodeUtility
{
    public record Node(float X, float Y, ushort Width, ushort Height, ushort PartId, uint PartCount);

    [GeneratedRegex("\\d+")]
    private static partial Regex NumberRegex();

    private static int Aetherpool(AtkUnitBase* addon, int index)
    {
        var componentNode = NodeUtility.GetAddonChildNode(addon, index)->GetAsAtkComponentNode();
        if (componentNode != null)
        {
            var manager = componentNode->Component->UldManager;
            for (var i = 0; i < manager.NodeListCount; i++)
            {
                var textNode = manager.NodeList[i]->GetAsAtkTextNode();
                if (textNode != null)
                {
                    var text = textNode->NodeText.ToString();
                    if (text != null)
                    {
                        if (int.TryParse(NodeUtility.NumberRegex().Match(text).Value, out var aetherpool))
                            return aetherpool;
                    }
                }
            }
        }
        return 0;
    }

    public static (bool, int, int) AetherpoolStatus(IGameGui gameGui)
    {
        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonStatus", 1)!;
        return (addon != null) ? (true, NodeUtility.Aetherpool(addon, 73), NodeUtility.Aetherpool(addon, 72)) : (false, -1, -1);
    }

    private static AtkResNode* GetAddonChildNode(AtkUnitBase* addon, int index)
    {
        if (addon == null)
            return null;
        return (index < addon->UldManager.NodeListCount) ? addon->UldManager.NodeList[index] : null;
    }

    private static AtkResNode* GetComponentChildNode(AtkComponentNode* componentNode, int index)
    {
        if (componentNode == null)
            return null;
        return (index < componentNode->Component->UldManager.NodeListCount) ? componentNode->Component->UldManager.NodeList[index] : null;
    }

    public static int SaveSlotNumber(IGameGui gameGui)
    {
        static AtkResNode* GetSlotNode(AtkUnitBase* addon, int index)
        {
            var componentNode = NodeUtility.GetAddonChildNode(addon, 2)->GetAsAtkComponentNode();
            componentNode = NodeUtility.GetComponentChildNode(componentNode, index)->GetAsAtkComponentNode();
            return NodeUtility.GetComponentChildNode(componentNode, 1);
        }

        var saveSlotNumber = 0;
        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonSaveData", 1)!;
        if (addon == null)
            return saveSlotNumber;

        var slot1Node = GetSlotNode(addon, 1);
        var slot2Node = GetSlotNode(addon, 2);

        if (slot1Node != null && slot2Node != null)
        {
            var r1 = slot1Node->AddRed;
            var r2 = slot2Node->AddRed;

            if (r1 > r2)
                saveSlotNumber = 1;
            else if (r2 > r1)
                saveSlotNumber = 2;
        }
        return saveSlotNumber;
    }

    public static (bool, bool) SaveSlotDeletion(IGameGui gameGui)
    {
        static bool IsEmpty(AtkTextNode* node) => node->NodeText.ToString().IsNullOrWhitespace();

        static AtkTextNode* GetSlotNodeData(AtkUnitBase* addon, int index)
        {
            var componentNode = NodeUtility.GetAddonChildNode(addon, 2)->GetAsAtkComponentNode();
            componentNode = NodeUtility.GetComponentChildNode(componentNode, index)->GetAsAtkComponentNode();
            var textNode = NodeUtility.GetComponentChildNode(componentNode, 19)->GetAsAtkTextNode();
            if (textNode != null && !IsEmpty(textNode))
            {
                var nextSiblingNode = textNode->AtkResNode.NextSiblingNode;
                if (nextSiblingNode != null)
                    return nextSiblingNode->GetAsAtkTextNode();
            }
            return null;
        }

        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonSaveData", 1)!;
        if (addon == null)
            return (false, false);

        var slot1NodeData = GetSlotNodeData(addon, 1);
        var slot2NodeData = GetSlotNodeData(addon, 2);

        if (slot1NodeData != null && slot2NodeData != null)
            return (IsEmpty(slot1NodeData), IsEmpty(slot2NodeData));

        return (false, false);
    }

    public static (bool, int) MapFloorNumber(IGameGui gameGui)
    {
        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonMap", 1)!;
        if (addon == null)
            return (false, -1);

        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var textNode = addon->UldManager.NodeList[i]->GetAsAtkTextNode();
            if (textNode == null)
                continue;

            if (int.TryParse(NodeUtility.NumberRegex().Match(textNode->NodeText.ToString()).Value, out var number))
                return (true, number);
            break;
        }
        return (false, -1);
    }

    public static IImmutableList<Node>? MapRoom(IGameGui gameGui)
    {
        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonMap", 1)!;
        if (addon == null)
            return null;

        IImmutableList<Node> nodes = ImmutableArray.Create<Node>();
        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var resNode = addon->UldManager.NodeList[i];
            if (resNode == null)
                continue;

            var componentNode = resNode->GetAsAtkComponentNode();
            if (componentNode == null)
                continue;

            var imageNode = componentNode->Component->UldManager.NodeListCount > 0 ? componentNode->Component->UldManager.NodeList[0]->GetAsAtkImageNode() : null;
            if (imageNode == null)
                continue;

            var x = resNode->X;
            var y = resNode->Y;
            if (x == 0.0f && y == 0.0f)
                continue;

            var id = imageNode->PartId;
            var count = imageNode->PartsList->PartCount;
            if ((id >= 0 && id < 16 && count == 16) || (id >= 0 && id < 9 && count == 9))
                nodes = nodes.Add(new(x, y, resNode->Width, resNode->Height, id, count));
        }
        return nodes;
    }

    public static bool CairnOfPassageActivation(IGameGui gameGui)
    {
        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonMap", 1)!;
        if (addon == null)
            return false;

        var skipFirst = false;
        for (var i = addon->UldManager.NodeListCount - 1; i >= 0; i--)
        {
            var resNode = addon->UldManager.NodeList[i];
            if (resNode == null)
                continue;

            var componentNode = resNode->GetAsAtkComponentNode();
            if (componentNode == null)
                continue;

            var imageNode = componentNode->Component->UldManager.NodeListCount > 1 ? componentNode->Component->UldManager.NodeList[1]->GetAsAtkImageNode() : null;
            if (imageNode == null)
                continue;

            if (imageNode->PartsList->PartCount == 11)
            {
                if (!skipFirst)
                {
                    skipFirst = true;
                    continue;
                }
                return imageNode->PartId == 10;
            }
        }
        return false;
    }

    private static (bool, int) ScoreWindowData(IGameGui gameGui, int index)
    {
        static (bool, int) GetValue(AtkComponentNode* node)
        {
            var buffer = string.Empty;
            for (var i = node->Component->UldManager.NodeListCount - 1; i >= 0; i--)
            {
                var resNode = node->Component->UldManager.NodeList[i]->GetAsAtkComponentNode();
                if (resNode == null)
                    continue;

                var imageNode = resNode->Component->UldManager.NodeListCount > 0 ? resNode->Component->UldManager.NodeList[0]->GetAsAtkImageNode() : null;
                if (imageNode == null)
                    continue;

                buffer += imageNode->PartId.ToString(CultureInfo.InvariantCulture);
            }
            return int.TryParse(buffer, out int value) ? (true, value) : (false, -1);
        }

        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("DeepDungeonResult", 1)!;
        if (addon == null)
            return (false, -1);

        var exitButton = addon->UldManager.NodeListCount > 2 ? addon->UldManager.NodeList[2]->GetAsAtkComponentButton() : null;
        if (exitButton == null || !exitButton->IsEnabled)
            return (false, -1);

        var floorNode = addon->UldManager.NodeListCount > 13 ? addon->UldManager.NodeList[13]->GetAsAtkComponentNode() : null;
        if (floorNode == null)
            return (false, -1);

        var result = GetValue(floorNode);
        if (!result.Item1 || result.Item2 <= 0)
            return (false, -1);

        var node = addon->UldManager.NodeListCount > index ? addon->UldManager.NodeList[index]->GetAsAtkComponentNode() : null;
        if (node == null)
            return (false, -1);

        return GetValue(node);
    }

    public static (bool, int) ScoreWindowKills(IGameGui gameGui) => NodeUtility.ScoreWindowData(gameGui, 11);

    public static (bool, int) ScoreWindowScorePoints(IGameGui gameGui) => NodeUtility.ScoreWindowData(gameGui, 9);

    public static bool IsNowLoading(IGameGui gameGui)
    {
        var addon = (AtkUnitBase*)gameGui?.GetAddonByName("NowLoading", 1)!;
        return addon != null && addon->IsVisible;
    }
}