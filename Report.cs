using System.Text.Json.Serialization;

namespace testeAuvo2;

public class DepartmentDTO {
    public string Departamento { get; set; }
    public int MesVigencia { get; set; }
    public int AnoVigencia { get; set; }
    public double TotalPagar { get; set; }
    public double TotalDescontos { get; set; }
    public double TotalExtras { get; set; }
    public ICollection<EmployeeDTO> Funcionarios { get; set; }
}

public class EmployeeDTO {
    public string Nome { get; set; }
    public long Codigo { get; set; }
    public double TotalReceber { get; set; }
    public double HorasExtras { get; set; }
    public double HorasDebito { get; set; }
    public int DiasFalta { get; set; }
    public int DiasExtras { get; set; }
    public int DiasTrabalhados { get; set; }
    [JsonIgnore]
    public List<ClockInsInputDTO> clockIns { get; set; }
}

public class ClockInsInputDTO {
    public long Codigo { get; set; }
    public string Nome { get; set; }
    public double ValorHora { get; set; }
    public DateTime Data { get; set; }
    public int HorasTrabalhadas { get; set; }
}