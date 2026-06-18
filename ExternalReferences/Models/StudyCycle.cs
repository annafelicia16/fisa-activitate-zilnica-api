namespace FisaActivitateZilnicaApi.ExternalReferences.Models;

// Study cycle of a program (Specializare). Values intentionally match AGSIS
// N_CICLU_STUDII.ID_ELEMENT / N_PROGRAM_DE_STUDIU.ID_CICLU_STUDII (1/2/3) so the
// program-cycle mapping is a straight cast.
public enum StudyCycle
{
    Unknown = 0,
    Bachelor = 1, // AGSIS: Licență
    Master = 2, // AGSIS: Master / Masterat
    Doctorate = 3, // AGSIS: Doctorat
}

// Centralizes the AGSIS-specific knowledge of how to derive a study cycle.
//
// dbo.Specializare.ID_TipCiclu (the obvious column) is 0/NULL for every row in
// this AGSIS instance, so it is deliberately ignored. The cycle is instead taken
// from the national RMU/ANS study-program nomenclature
// (Specializare.id_n_programdestudiu → N_PROGRAM_DE_STUDIU.ID_CICLU_STUDII) and,
// when that link is missing, falls back to the awarded diploma type
// (Specializare.ID_N_Tip_Diploma_Universitar → N_Tip_Diploma_Universitar).
public static class StudyCycleResolver
{
    // N_PROGRAM_DE_STUDIU.ID_CICLU_STUDII: 1 = Licență, 2 = Master, 3 = Doctorat.
    public static StudyCycle FromProgramCycle(int? idCicluStudii) =>
        idCicluStudii switch
        {
            1 => StudyCycle.Bachelor,
            2 => StudyCycle.Master,
            3 => StudyCycle.Doctorate,
            _ => StudyCycle.Unknown,
        };

    // N_Tip_Diploma_Universitar.ID_Element. Only the unambiguous diploma types are
    // mapped; combined/legacy/"alte situații" types stay Unknown rather than risk a
    // wrong classification.
    public static StudyCycle FromDiplomaType(int? idTipDiplomaUniversitar) =>
        idTipDiplomaUniversitar switch
        {
            2 or 15 or 17 or 20 => StudyCycle.Bachelor, // licență / inginer / urbanist / arhitect
            5 or 7 => StudyCycle.Master, // diplomă / adeverință de master
            8 or 9 => StudyCycle.Doctorate, // diplomă / echivalare doctor
            _ => StudyCycle.Unknown,
        };

    // Program-cycle is authoritative; diploma type is the fallback.
    public static StudyCycle Resolve(int? programCycle, int? diplomaType)
    {
        StudyCycle cycle = FromProgramCycle(programCycle);
        return cycle != StudyCycle.Unknown ? cycle : FromDiplomaType(diplomaType);
    }

    // Human-readable label for API responses; null when the cycle is unknown.
    public static string? ToLabel(StudyCycle cycle) =>
        cycle switch
        {
            StudyCycle.Bachelor => "Bachelor",
            StudyCycle.Master => "Master",
            StudyCycle.Doctorate => "Doctorate",
            _ => null,
        };
}
