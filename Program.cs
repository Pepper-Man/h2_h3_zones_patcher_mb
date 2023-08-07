using Bungie;
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class MB_Zones
{
    static void Main(string[] args)
    {
        // Variables
        string h3ek_path = @"C:\Program Files (x86)\Steam\steamapps\common\H3EK";
        string xml_path = @"G:\Steam\steamapps\common\H2EK\04a_gasgiant.xml";

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);

        Convert_XML(xml_path);
    }

    static void Convert_XML(string xml_path)
    {
        Console.WriteLine("Beginning XML Conversion:\n\n");

        XmlDocument scenfile = new XmlDocument();
        scenfile.Load(xml_path);

        XmlNode root = scenfile.DocumentElement;

        XmlNodeList zones_start_blocks = root.SelectNodes(".//block[@name='zones']");

        List<string> zones_list = new List<string>();

        // Get all zone names
        foreach (XmlNode zones_block in zones_start_blocks)
        {
            bool zones_end = false;
            int i = 0;
            while (!zones_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = zones_block.SelectSingleNode(search_string);

                if (element != null)
                {
                    string zoneName = element.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    zones_list.Add(zoneName);
                    Console.WriteLine(zoneName);
                    i++;
                }
                else
                {
                    zones_end = true;
                }
            }
        }

        // Temp file creation/wiping

        using (FileStream fs = new FileStream("temp_output.txt", FileMode.Create, FileAccess.Write))
        using (StreamWriter sw = new StreamWriter(fs))
        {
            // Get areas and firing positions for each name
            int i = 0;
            foreach (string zone_name in zones_list)
            {
                // Areas
                sw.WriteLine("Zone: " + zone_name + "\nAreas:");
                XmlNodeList areasBlockList = root.SelectNodes(".//block[@name='zones']/element[@index='" + i + "']/block[@name='areas']");
                foreach (XmlNode area_block in areasBlockList)
                {
                    bool areas_end = false;
                    int j = 0;
                    while (areas_end == false)
                    {
                        string search_string = "./element[@index='" + j.ToString() + "']";
                        var element = area_block.SelectSingleNode(search_string);

                        if (element != null)
                        {
                            sw.WriteLine(element.SelectSingleNode("./field[@name='name']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='area flags']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='runtime starting index']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='runtime count']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='manual reference frame']").InnerText.Trim());
                            j++;
                        }
                        else
                        {
                            areas_end = true;
                        }
                    }
                }

                // Firing Positions
                sw.WriteLine("Firing Positions:");
                XmlNodeList fpos_block_list = root.SelectNodes(".//block[@name='zones']/element[@index='" + i + "']/block[@name='firing positions']");
                foreach (XmlNode fpos_block in fpos_block_list)
                {
                    bool fpos_end = false;
                    int j = 0;
                    while (fpos_end == false)
                    {
                        string search_string = "./element[@index='" + j.ToString() + "']";
                        var element = fpos_block.SelectSingleNode(search_string);

                        if (element != null)
                        {
                            sw.WriteLine("index = " + j.ToString());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='position (local)']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='reference frame']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='flags']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='cluster index']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='normal']").InnerText.Trim());
                            j++;
                        }
                        else
                        {
                            fpos_end = true;
                        }
                    }
                }
                i++;
            }
        }
        Edit_The_Tags();
    }

    static void Edit_The_Tags()
    {
        Console.WriteLine("Beginning tag writing process...");

        int zone = -1;
        bool areas = false;
        bool fpos = false;
        int areaIndex = -1;
        int fposIndex = -1;
        int areaDataCount = 0;
        int fposDataCount = 0;
        int fposFlagLineSkip = 0;
        bool previousWasActualFlag = false;

        string[] text = File.ReadAllLines("temp_output.txt");

        foreach (string line in text)
        {
            if (line.Contains("Zone:"))
            {
                zone++;
                Console.WriteLine(line.Trim());
                PatchTag("zone", line.Substring(line.IndexOf(' ') + 1).Trim());
                continue;
            }
            else if (line.Contains("Areas:"))
            {
                areas = true;
                fpos = false;
                areaIndex = 0;
                areaDataCount = 0;
                continue;
            }
            else if (line.Contains("Firing Positions:"))
            {
                fpos = true;
                areas = false;
                fposDataCount = 0;
                fposIndex = 0;
                continue;
            }
            else if (areas)
            {
                if (previousWasActualFlag)
                {
                    previousWasActualFlag = false;
                    continue;
                }
                if (areaDataCount == 0)
                {
                    // Patch area name
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].name";
                    Console.WriteLine($"patching {line.Trim()}");
                    //PatchTag(fieldPath, line.Trim());
                    areaDataCount++;
                }
                else if (areaDataCount == 1)
                {
                    // Patch area flags
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].area flags";
                    Console.WriteLine("area flags");
                    //PatchTag(fieldPath, line.Trim());
                    areaDataCount++;
                    if (line.Trim() != "0")
                    {
                        previousWasActualFlag = true;
                    }
                }
                else if (areaDataCount == 2)
                {
                    // line is runtime starting index
                    areaDataCount++;
                    continue;
                }
                else if (areaDataCount == 3)
                {
                    // line is runtime count
                    areaDataCount++;
                    continue;
                }
                else if (areaDataCount == 4)
                {
                    // Patch manual reference frame
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].manual reference frame";
                    Console.WriteLine($"manual ref frame = {line.Trim()}");
                    //PatchTag(fieldPath, ",-1");
                    areaDataCount = 0;
                    areaIndex++;
                }
            }
            else if (fpos)
            {
                if (fposFlagLineSkip > 0)
                {
                    fposFlagLineSkip--;
                    continue;
                }
                if (fposDataCount == 0)
                {
                    // Patch firing position index
                    Console.WriteLine($"firing position {line.Trim()} patch start");
                    fposDataCount++;
                }
                else if (fposDataCount == 1)
                {
                    // Patch firing position position
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].position (local)";
                    Console.WriteLine("patching position");
                    //PatchTag(fieldPath, line.Trim());
                    fposDataCount++;
                }
                else if (fposDataCount == 2)
                {
                    // Patch firing position ref frame
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].reference frame";
                    Console.WriteLine("patching ref frame");
                    //PatchTag(fieldPath, ",-1");
                    fposDataCount++;
                }
                else if (fposDataCount == 3)
                {
                    // Patch firing position flags
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].flags";
                    Console.WriteLine("patching flags");
                    //PatchTag(fieldPath, Regex.Replace(line.Trim(), "[^0-9]", ""));
                    if (int.Parse(line.Trim()) >= 90)
                    {
                        fposFlagLineSkip = 4;
                    }
                    else if (int.Parse(line.Trim()) > 70)
                    {
                        fposFlagLineSkip = 3;
                    }
                    else if (int.Parse(line.Trim()) < 9 && int.Parse(line.Trim()) >= 1)
                    {
                        fposFlagLineSkip = 1;
                    }
                    else if (int.Parse(line.Trim()) == 0)
                    {
                        fposFlagLineSkip = 0;
                    }
                    else
                    {
                        fposFlagLineSkip = 2;
                    }
                    fposDataCount++;
                }
                else if (fposDataCount == 4)
                {
                    // Patch firing position area
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].area";
                    Console.WriteLine("patching area");
                    //PatchTag(fieldPath, "," + line.Trim());
                    fposDataCount++;
                }
                else if (fposDataCount == 5)
                {
                    // Patch firing position cluster index
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].cluster index";
                    Console.WriteLine("patching cluster index");
                    //PatchTag(fieldPath, line.Trim());
                    fposDataCount++;
                }
                else if (fposDataCount == 6)
                {
                    // Patch firing position normal
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].normal";
                    Console.WriteLine("patching normal");
                    //PatchTag(fieldPath, line.TrimEnd('\n'));
                    fposDataCount = 0;
                    fposIndex++;
                }
            }
        }
    }

    static void PatchTag(string type, string line)
    {
        string scen_path = @"halo_2\levels\singleplayer\04a_gasgiant\04a_gasgiant";
        var tag_path = Bungie.Tags.TagPath.FromPathAndType(scen_path, "scnr*");

        // Managedblam to insert new field entries
        if (type == "zone")
        {
            // Add a new zone
            using (var tagFile = new Bungie.Tags.TagFile(tag_path))
            {
                // Add new zone entry
                int zones_max_index = ((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements.Count() - 1;
                ((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).AddElement(); // 83 is the position of the Zones block
                var zone_name = (Bungie.Tags.TagFieldElementString)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[zones_max_index + 1].Fields[0];
                zone_name.Data = line;
                tagFile.Save();
                
            }
        }

        
    }
}