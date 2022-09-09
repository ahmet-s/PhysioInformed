INCLUDE CC
INCLUDE ADMP_Sur
INCLUDE ADMP_Phy
INCLUDE ADMP_Inj
INCLUDE ADMP_AS
INCLUDE ADMP_NSA
INCLUDE ADMP_WaS
INCLUDE ADMP_IaP
INCLUDE ADMP_SaP

EXTERNAL EndConsultation()

LIST speaker = Doctor, Patient
VAR patientDiversionIndex = 0

LIST treatments = Inj = 5, Phy = 3, Sur = 8, WaS = 4, AS = 0, NSA = 6, IaP = 1, SaP = 2
LIST currentTreatments = (X)
VAR treatmentsToNegotiate = 2

VAR preparedTreatments = false
VAR lastTreatment = "X"
VAR bestTreatment = X
VAR worstTreatment = X
VAR middleTreatment = X

LIST _ADMP_OSCEPoints = Consult, Share, Explore

VAR prescribedTreatment = ""

/////******************Patient Elimination/Prescribtion Respond**********

==P_Eliminate==
~speaker = Patient
- Okay Doctor.
->DONE

==P_Prescribe==
~speaker = Patient
- Okay Doctor, if that's the best.
->D_CC_0



/////***************************FUNCTIONS***************************

==function NegotiatedTreatment==
~return 0

==function TreatmentValues==
{
- LIST_COUNT(currentTreatments) == 3:
~bestTreatment = LIST_MIN(currentTreatments)
~worstTreatment = LIST_MAX(currentTreatments)
~middleTreatment = LIST_RANGE(currentTreatments, LIST_VALUE(bestTreatment)+1, LIST_VALUE(worstTreatment)-1)
- else:
~bestTreatment = LIST_MIN(currentTreatments)
~worstTreatment = LIST_MAX(currentTreatments)
}

==function TreatmentFullNames(name)==
{
- name == AS:
~return "advice sheets"
- name == Sur:
~return "surgery"
- name == Phy:
~return "physiotherapy"
- name == NSA:
~return "NSAIDs"
- name == Inj:
~return "injection"
- name == WaS:
~return "wait and see"
- name == IaP:
~return "injection with physiotherapy"
- name == SaP:
~return "surgery with physiotherapy"
}

==function Prescription(name)==
{
- name == AS:
~return "Advice Sheets"
- name == Sur:
~return "Surgery"
- name == Phy:
~return "Physiotherapy"
- name == NSA:
~return "NSAIDs"
- name == Inj:
~return "Injection"
- name == WaS:
~return "Wait and See"
- name == IaP:
~return "Injection and Physiotherapy"
- name == SaP:
~return "Surgery and Physiotherapy"
}

==function EndConsultation==
~return 0

