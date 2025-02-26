public class Consulta
{
    public double AlturaMax { get; set; }
    public double AlturaMin { get; set; }
    public string Hexid { get; set; }

    public Consulta(double alturaMax, double alturaMin, string hexid)
    {
        AlturaMax = alturaMax;
        AlturaMin = alturaMin;
        Hexid = hexid;
    }
}
