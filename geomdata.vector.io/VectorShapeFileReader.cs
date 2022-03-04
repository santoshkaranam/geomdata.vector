using System;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace geomdata.vector.io
{
    public class VectorShapeFileReader : VectorFileReader
    {
        private readonly ShapefileDataReader _reader;

        public VectorShapeFileReader(string path)
        {
            _reader = new ShapefileDataReader(path,
                new GeometryFactory(new PrecisionModel(), GeometryWgs84Srid));
        }

        public override bool Read() => _reader.Read();

        public override object GetValue(int index) => _reader.GetValue(index+1);

        public override Envelope GetBounds() => _reader.ShapeHeader.Bounds;

        public override ShapeGeometryType GetShapeGeometryType() => _reader.ShapeHeader.ShapeType;

        public override int GetOrdinal(string columnName) => _reader.GetOrdinal(columnName);

        public override string[] GetAllFieldNames() => _reader.DbaseHeader.Fields.Select(x => x.Name).ToArray();

        public override int GetInt32(int index) => _reader.GetInt32(index);

        public override double GetDouble(int index) => _reader.GetDouble(index);

        public override string GetString(int index) => _reader.GetString(index);

        public override DateTime GetDateTime(int index) => _reader.GetDateTime(index);

        public override int GetSrsId() => GeometryWgs84Srid;

        public override void Dispose() => _reader.Dispose();

        public override Geometry Geometry => _reader.Geometry;
        public override int FieldCount() => _reader.DbaseHeader.Fields.Length;
        public override string GetFieldName(int index) => _reader.DbaseHeader.Fields[index].Name;
    }
}