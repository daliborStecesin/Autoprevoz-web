namespace Transport.Application.Interfaces;

public interface IProfilService
{
    /// <summary>
    /// tbl_web_licence.TipPrograma ("TRANSPORT"/"TRGOVINA") za trenutnu firmu (ap_idfirme).
    /// Štampe i forme ovo nikad ne čitaju direktno — pitaju JeTransport()/Prikazi*() ispod.
    /// </summary>
    Task<string> TipPrograma();

    /// <summary>Da li je trenutna firma profila TRANSPORT.</summary>
    Task<bool> JeTransport();

    /// <summary>Da li se na štampi računa prikazuje "Datum utovara" (u zaglavlju).</summary>
    Task<bool> PrikaziDatumUtovara();
}
