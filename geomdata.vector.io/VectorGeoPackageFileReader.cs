using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace geomdata.vector.io
{
    public class VectorGeoPackageFileReader : VectorFileReader
    {
        private readonly SqliteConnection _connection;
        private readonly string _tableName;
        private readonly SqliteDataReader _reader;
        private readonly int _srsId;
        private readonly Envelope _envelope;

        public VectorGeoPackageFileReader(string path)
        {
            _connection = new SqliteConnection($"Data Source=\"{path}\"");

            _connection.Open();
            var tableNameCommand = _connection.CreateCommand();
            tableNameCommand.CommandText = @"SELECT * FROM gpkg_contents;";
            using (var tableReader = tableNameCommand.ExecuteReader())
            {
                while (tableReader.Read())
                {
                    _tableName = tableReader.GetString(tableReader.GetOrdinal("table_name"));
                    _srsId = tableReader.GetInt32(tableReader.GetOrdinal("srs_id"));

                    var minX = tableReader.GetDouble(tableReader.GetOrdinal("min_x"));
                    var minY = tableReader.GetDouble(tableReader.GetOrdinal("min_x"));
                    var maxX = tableReader.GetDouble(tableReader.GetOrdinal("max_x"));
                    var maxY = tableReader.GetDouble(tableReader.GetOrdinal("max_y"));
                    _envelope = new Envelope(minX, maxX, minY, maxY);
                }
            }

            var entityCommand = _connection.CreateCommand();
            entityCommand.CommandText = $@"SELECT * FROM [{_tableName}];";
            _reader = entityCommand.ExecuteReader();
        }

        public override void Dispose()
        {
            _connection.Close();
            _reader.Dispose();
            _connection.Dispose();
        }

        public override bool Read() => _reader.Read();

        public override object GetValue(int index) => _reader.GetValue(index);

        public override Envelope GetBounds() => _envelope;

        public override ShapeGeometryType GetShapeGeometryType()
        {
            var tableNameCommand = _connection.CreateCommand();
            tableNameCommand.CommandText = "SELECT * FROM gpkg_geometry_columns";
            var tableReader = tableNameCommand.ExecuteReader();
            var type = "";
            while (tableReader.Read())
            {
                type = tableReader.GetString(tableReader.GetOrdinal("geometry_type_name"));
            }

            if (type == "POINT")
            {
                return ShapeGeometryType.Point;
            }

            if (type == "POLYGON")
            {
                return ShapeGeometryType.Polygon;
            }

            if (type == "MULTIPOLYGON")
            {
                return ShapeGeometryType.Polygon;
            }

            throw new InvalidDataException($"Invalid geometry type while reading ShapeGeometryType");
        }

        public override int GetOrdinal(string columnName) => _reader.GetOrdinal(columnName);

        public override string[] GetAllFieldNames()
        {
            var count = _reader.FieldCount;
            var names = new List<string>();
            for (int i = 0; i < count; i++)
            {
                names.Add(_reader.GetName(i));
            }

            return names.ToArray();
        }

        public override string GetFieldName(int index) => _reader.GetName(index);

        public override int FieldCount() => _reader.FieldCount;

        public override int GetInt32(int index) => _reader.GetInt32(index);

        public override double GetDouble(int index) => _reader.GetDouble(index);

        public override string GetString(int index) => _reader.GetString(index);

        public override DateTime GetDateTime(int index) => _reader.GetDateTime(index);

        public override int GetSrsId() => _srsId;

        public override Geometry Geometry
        {
            get
            {
                var geometryReader = new GeoPackageGeoReader();
                return geometryReader.Read(_reader.GetStream(_reader.GetOrdinal("geom")));
            }
        }
    }
}