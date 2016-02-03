using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DipProg
{
    public class Fuzzification
    {
        private readonly List<Description> m_Descriptions;
        private readonly DataTable m_Data;
        private readonly List<Tuple<List<double>, List<double>>> result;

        public Fuzzification(List<Description> descriptions, DataTable data)
        {
            m_Descriptions = descriptions;
            m_Data = data;
            result = new List<Tuple<List<double>, List<double>>>();
        }

        public void Calculate()// relative frequency
        {
            foreach(var desc in m_Descriptions)
            {
                if (!desc.IsNotUse)
                {
                    List<double> relativeFrequencies = new List<double>();
                    var column = m_Data.Columns[desc.TableName];
                    foreach (var value in desc.Values)
                    {
                        double itemCount = m_Data.AsEnumerable().Where(x => x.Field<string>(desc.TableName) == value).Count();
                        relativeFrequencies.Add(itemCount / m_Data.Rows.Count);
                    }

                    List<double> cumulativeFrequency = new List<double>();
                    double sum = 0.0;
                    for(int l=0; l<relativeFrequencies.Count; l++)
                    {
                        double relativeFrequency = relativeFrequencies[l];
                        cumulativeFrequency.Add(relativeFrequency/2+sum);
                        sum += relativeFrequency;
                    }

                    result.Add(Tuple.Create(relativeFrequencies, cumulativeFrequency));
                }
            }
        }
    }
}
