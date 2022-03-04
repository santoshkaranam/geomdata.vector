using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using geomdata.vector.io;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace geomdata.vector.split
{
    class Program
    {
        static Program()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Log.Information("Welcome to Vector Dataset Split Utility");
                Log.Information(
                    "Enter path of Large Shape file (.shp) or Geopackage (.gpkg) file to split into smaller files. Only EPSG4326 supported");
                var filePath = GetInput();
                var fileInfo = new FileInfo(filePath);
                if (File.Exists(filePath) == false &&
                    new[] {"shp", "gpkg"}.Contains(fileInfo.Extension) == false)
                {
                    Log.Error("Invalid File Path entered!");
                    Environment.Exit(-1);
                }

                Log.Information("Enter the count of features per file");
                var count = int.Parse(GetInput());
                VectorOps.GetVectorSplitter(filePath, count,
                    $"{fileInfo.FullName.Replace(fileInfo.Extension, string.Empty)}");
            }

            Log.CloseAndFlush();
        }

        private static string GetInput()
        {
            return Console.ReadLine();
        }
    }

    public static class VectorOps
    {
        public const string EPsg4326EsriWkt =
            "GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]]";

        public static void GetVectorSplitter(string filePath, int count, string outFileName)
        {
            var reader = VectorFileReader.GetReader(filePath);
            Log.Information($"Geometry type - {reader.GetShapeGeometryType()}");
            var fieldCount = reader.FieldCount();
            Log.Information($"FieldCount-{fieldCount}");
            var fieldNames = GetFieldNames(fieldCount, reader);

            var features = new FeatureCollection();
            var batchFeatureCount = 0;
            var fileIndex = 1;
            while (reader.Read())
            {
                var attributes = new AttributesTable();
                foreach (var key in fieldNames.Keys)
                {
                    var value = reader.GetValue(key) ?? "null";
                    if (value is DBNull) // fix fo geo-package
                    {
                        value = "null";
                    }

                    attributes.Add(fieldNames[key], value);
                }

                var feature = new Feature(reader.Geometry, attributes);
                features.Add(feature);
                batchFeatureCount++;
                if (batchFeatureCount == count)
                {
                    CreateFile(outFileName, fileIndex++, features);
                    features = new FeatureCollection();
                    batchFeatureCount = 0;
                }
            }

            if (features.Any())
            {
                CreateFile(outFileName, fileIndex, features);
            }
        }

        private static void CreateFile(string outFileName, int fileIndex, FeatureCollection features)
        {
            var fileName = $"{outFileName}_part{fileIndex}";
            var featuresCount = features.Count;
            Log.Information($"Writing to {featuresCount} file-{fileName}.");
            var writer = new ShapefileDataWriter(fileName)
            {
                Header = ShapefileDataWriter.GetHeader(features.First(), featuresCount)
            };
            writer.Write(features);
            File.WriteAllText($"{fileName}.prj", EPsg4326EsriWkt);
        }

        private static Dictionary<int, string> GetFieldNames(int fieldCount, VectorFileReader reader)
        {
            var fieldNames = new Dictionary<int, string>();
            for (int index = 0; index < fieldCount; index++)
            {
                var fieldName = reader.GetFieldName(index);
                Log.Information($"FieldName-{index}. {fieldName}");
                if (fieldName == "geom")
                {
                    Log.Warning($"Skipping column-{fieldName}");
                    continue;
                }

                fieldNames.Add(index, fieldName.Length > 11 ? fieldName.Substring(0, 11) : fieldName);
            }

            return fieldNames;
        }
    }
}