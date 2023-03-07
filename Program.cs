using System.Globalization;
using System.Text;
using System.Text.Json;

namespace testeAuvo2;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Digite por favor o caminho onde encontram-se os arquivos:");
        string path = Console.ReadLine();
        if (path != null) {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            var departments = new List<DepartmentDTO>();
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file => {
                departments.Add(ReadFile(file));
            });
            
            Console.WriteLine(JsonSerializer.Serialize(departments));
        }
    }

    static DepartmentDTO ReadFile(FileInfo file) {
        var filename = file.FullName.Split("\\").Last();
        var parts = filename.Split("-");
        if (parts.Length != 3)
            throw new Exception($"O nome do arquivo {filename} está fora do padrão (Departamento-Mês-Ano.csv).");
        var departmentName = parts[0];
        Enum.TryParse(parts[1].ToString(), out Months month);
        var mes = (int)month;
        var ano = int.Parse(parts[2].Replace(".csv", ""));
                
        var department = new DepartmentDTO() {
            Departamento = departmentName,
            MesVigencia = mes,
            AnoVigencia = ano,
            TotalPagar = 0,
            TotalDescontos = 0,
            TotalExtras = 0,
            Funcionarios = new List<EmployeeDTO>()
        };

        using (StreamReader sr = file.OpenText())
        {
            string s = "";
            sr.ReadLine();
            while ((s = sr.ReadLine()) != null)
            {
                if (s != null) {
                    var clockIn = ReadClockIn(s);
                    var employee = department.Funcionarios.FirstOrDefault(x => x.Codigo == clockIn.Codigo);
                    if (employee == null) {
                        employee = new EmployeeDTO {
                            Codigo = clockIn.Codigo,
                            Nome = clockIn.Nome,
                            TotalReceber = 0,
                            HorasExtras = 0,
                            HorasDebito = 0,
                            DiasFalta = 0,
                            DiasExtras = 0,
                            DiasTrabalhados = 0,
                            clockIns = new List<ClockInsInputDTO>()
                        };
                        employee.clockIns.Add(clockIn);
                        department.Funcionarios.Add(employee);
                    } else {
                        employee.clockIns.Add(clockIn);
                    }  
                }
            }
        }

        foreach (var employee in department.Funcionarios) {
            var dataInicio = new DateTime(ano, mes, 01, 00, 00, 00);
            var dataFim = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes), 23, 59, 59);
                        
            var dataAtual = dataInicio;
            while (dataAtual <= dataFim) {
                var clockin = employee.clockIns.First(x => x.Data.Date == dataAtual);
                bool diaTrabalhado = (clockin != null);
                
                if (dataAtual.DayOfWeek != DayOfWeek.Saturday && dataAtual.DayOfWeek != DayOfWeek.Sunday) {
                    employee.DiasTrabalhados += (diaTrabalhado) ? 1 : 0;
                    employee.DiasFalta += (!diaTrabalhado) ? 1 : 0;
                    if (clockin.HorasTrabalhadas >= 8) {
                        employee.HorasExtras += clockin.HorasTrabalhadas - 8;
                        department.TotalExtras += (clockin.HorasTrabalhadas - 8) * clockin.ValorHora;
                    } else {
                        employee.HorasDebito += 8 - clockin.HorasTrabalhadas;
                        department.TotalDescontos += (8 - clockin.HorasTrabalhadas) * clockin.ValorHora;
                    }
                } else {
                    if (clockin.HorasTrabalhadas >= 8) {
                        employee.DiasExtras += 1;
                        employee.HorasExtras += clockin.HorasTrabalhadas - 8;
                    } else {
                        employee.HorasExtras += 8 - clockin.HorasTrabalhadas;
                    }
                    department.TotalExtras += clockin.HorasTrabalhadas * clockin.ValorHora;
                }
                employee.TotalReceber += clockin.HorasTrabalhadas * clockin.ValorHora;
                department.TotalPagar += clockin.HorasTrabalhadas * clockin.ValorHora;
                dataAtual = dataAtual.AddDays(1);
            }
        }
        
        return department;
    }

    static ClockInsInputDTO ReadClockIn(string line) {
        var l = line.Split(";");
        var d = l[3].Split("/");
        DateTime date = new DateTime(int.Parse(d[2]), int.Parse(d[1]), int.Parse(d[0]));
        var e = l[4].Split(":");
        var entrada = new DateTime(date.Year, date.Month, date.Day, int.Parse(e[0]), int.Parse(e[1]), int.Parse(e[2]));
        var s = l[5].Split(":");
        var saida = new DateTime(date.Year, date.Month, date.Day, int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
        var alm = l[6].Split(" - ");
        var iAlm = alm[0].Split(":");
        var idaAlmoco = new DateTime(date.Year, date.Month, date.Day, int.Parse(iAlm[0]), int.Parse(iAlm[1]), 00);
        var vAlm = alm[1].Split(":");
        var voltaAlmoco = new DateTime(date.Year, date.Month, date.Day, int.Parse(vAlm[0]), int.Parse(vAlm[1]), 00);

        return new ClockInsInputDTO {
            Codigo = long.Parse(l[0]),
            Nome = l[1],
            ValorHora = double.Parse(l[2].Replace("R$", "").Replace(" ", "").Replace(",", "."), CultureInfo.InvariantCulture),
            Data = date,
            HorasTrabalhadas = (saida - entrada - (voltaAlmoco - idaAlmoco)).Hours
        };
    }
}
