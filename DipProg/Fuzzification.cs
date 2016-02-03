using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DipProg
{
    class ValueItem
    {
        public string Name { get; set; }
        public double FrequencyRelative { get; set; }
        public double FrequencyCumulative { get; set; }
    }

    public class Fuzzification
    {
        private readonly List<Description> m_Descriptions;
        private readonly DataTable m_Data;
        private readonly Dictionary<string, List<ValueItem>> result;
        private readonly DataTable m_ResultData;

        public Fuzzification(List<Description> descriptions, DataTable data)
        {
            m_Descriptions = descriptions;
            m_Data = data;
            result = new Dictionary<string, List<ValueItem>>();

            m_ResultData = m_Data.Copy();
        }

        public DataTable Calculate()// relative frequency
        {
            foreach(var desc in m_Descriptions)
            {
                if (!desc.IsNotUse)
                {
                    result[desc.TableName] = new List<ValueItem>();
                    foreach (var value in desc.Values)
                    {
                        double itemCount = m_Data.AsEnumerable().Where(x => x.Field<string>(desc.TableName) == value).Count();
                        result[desc.TableName].Add(new ValueItem() { Name = value, FrequencyRelative = itemCount / m_Data.Rows.Count });
                    }

                    List<double> cumulativeFrequency = new List<double>();
                    double sum = 0.0;
                    for(int l=0; l< result[desc.TableName].Count; l++)
                    {
                        double relativeFrequency = result[desc.TableName][l].FrequencyRelative;
                        result[desc.TableName][l].FrequencyCumulative = relativeFrequency/2+sum;
                        sum += relativeFrequency;
                    }
                }
            }

            foreach (var key in result.Keys)
            {
                m_ResultData.Columns[key].ReadOnly = false;
                foreach (var vi in result[key])
                {
                    var data = m_ResultData.AsEnumerable().Where(x => x.Field<string>(key) == vi.Name);
                    foreach (DataRow item in data)
                    {
                        item.SetField(key, vi.FrequencyCumulative);
                    }
                }                
            }

            return m_ResultData;
        }
    }
}
