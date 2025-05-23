using System.ComponentModel.DataAnnotations;

public class Monitor
{
    [Key] // opcional caso a primary key tenha o nome Id, Id<classe> ou <classe>Id
    public int IdMonitor { get; set; }
    public string RA { get; set; } = null!; // garante que não será nulo
    public string Nome { get; set; } = null!;
    public string Apelido { get; set; } = null!;
    public List<Horario> Horarios { get; set; } = [];
}
