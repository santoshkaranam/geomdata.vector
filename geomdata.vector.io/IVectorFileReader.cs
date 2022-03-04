using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace geomdata.vector.io
{
    public interface IVectorFileReader:IDisposable
    {
        bool Read();
        object GetValue(int index);
        Envelope GetBounds();
        ShapeGeometryType GetShapeGeometryType();
        int GetOrdinal(string columnName);
        string[] GetAllFieldNames();
        int GetInt32(int index);
        double GetDouble(int index);
        string GetString(int index);
        DateTime GetDateTime(int index);
        int GetSrsId();
        
        Geometry Geometry { get; }
        int FieldCount();
        string GetFieldName(int index);
    }
}