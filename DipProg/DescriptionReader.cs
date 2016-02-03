using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DipProg
{
    public class DescriptionReader : IDisposable
    {
        private readonly List<Description> m_Description = new List<Description>();
        private readonly XDocument m_XDoc;

        public DescriptionReader(string fileName)
        {
            m_XDoc = XDocument.Load(fileName);
            var fields = from field in m_XDoc.Descendants("Field")
                         select new Description
                         {
                             Name = field.Attribute("name").Value,
                             TableName = field.Attribute("tableName").Value,
                             IsNotUse = field.Attribute("nouse") != null ? Convert.ToBoolean(field.Attribute("nouse").Value) : false,
                             Values = field.Element("values").Descendants("value").Select(x => x.Value).ToList()
                         };
            m_Description = fields.ToList();
        }

        public List<Description> GetDescriptions()
        {
            return m_Description;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DescriptionReader() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
