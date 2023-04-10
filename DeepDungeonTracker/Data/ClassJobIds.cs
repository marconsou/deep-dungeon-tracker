using System.Collections.Generic;

namespace DeepDungeonTracker;

public static class ClassJobIds
{
    public static IDictionary<uint, (uint, string)> Data { get; }

    static ClassJobIds()
    {
        ClassJobIds.Data = new Dictionary<uint, (uint, string)>()
        {
            { 1, ( 0, "GLA")}, {19, ( 0, "PLD")},
            { 2, ( 1, "PGL")}, {20, ( 1, "MNK")},
            { 3, ( 2, "MRD")}, {21, ( 2, "WAR")},
            { 4, ( 3, "LNC")}, {22, ( 3, "DRG")},
            { 5, ( 4, "ARC")}, {23, ( 5, "BRD")},
            { 6, ( 6, "CNJ")}, {24, ( 6, "WHM")},
            { 7, ( 7, "THM")}, {25, ( 7, "BLM")},
            {26, ( 8, "ACN")}, {27, ( 9, "SMN")}, {28, (10, "SCH")},
            {29, (11, "ROG")}, {30, (11, "NIN")},
            {31, (12, "MCH")},
            {32, (13, "DRK")},
            {33, (14, "AST")},
            {34, (15, "SAM")},
            {35, (16, "RDM")},
            {36, (17, "BLU")},
            {37, (18, "GNB")},
            {38, (19, "DNC")},
            {39, (20, "RPR")},
            {40, (21, "SGE")}
        };
    }
}