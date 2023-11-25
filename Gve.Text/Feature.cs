using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public class Feature
{
    public string Name { get; set; }
    public string Value { get; set; }

    public Feature()
    {
        Name = "";
        Value = "";
    }

    public override string ToString()
    {
        return $"{ Name}={ Value}";
    }
}
