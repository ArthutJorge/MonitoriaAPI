using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Horario
{
    [Key] // opcional caso a primary key tenha o nome Id, Id<classe> ou <classe>Id
    public int IdHorario { get; set; }
    public int DiaSemana { get; set; }
    public string HorarioMonitoria { get; set; } = null!;
    public int IdMonitor { get; set; }
}
