INCLUDE IS
INCLUDE IRfC
INCLUDE BR
INCLUDE PfSDM

LIST storyFlow = (IS), IRfC, BR, PfSDM

LIST speaker = (Patient), Doctor
VAR patientDiversionIndex = 0
VAR transition = "none"

VAR treatmentRecommendationIndex = 0
VAR recommendInPfSDM = false
VAR recommendedTreatment = false
VAR prescribedTreatment = ""


//StrConsul_OSCE
VAR totalStrConsul = 1
VAR playerStrConsul = 0

EXTERNAL GoToGTO()
EXTERNAL ChangeFocusTopics()
EXTERNAL FinalizeSession()
EXTERNAL RecommendTreatment()
EXTERNAL TakingFocus()

->P_IS_0



==TreatmentRecommendation==
~speaker = Doctor
{
- storyFlow == IS:
Hi! As a treatment in your case, I recommend you to take <>
- storyFlow == IRfC:
I see. As a treatment I recommend you to use <>
- storyFlow == BR || PfSDM:
    {
    - recommendInPfSDM == true:
    Sure I can! Okay then, it would be good to take <>
    - else:
    Okay then. As a treatment I recommend you to use <>
    }
}
->Treatments

=Treatments
{
- treatmentRecommendationIndex == 0:
~ prescribedTreatment = Prescription("Inj") 
injection. Here, streroids are injected in the shoulder area. It's a synthetic anti-inflamatory drug and will reduce the pain.Sometimes, after 6 months, it effects can fade but if you be careful with your shoulder, it lasts longer. You can arrange a date for the procedure at the front desk.
- treatmentRecommendationIndex == 1:
~ prescribedTreatment = Prescription("Phy") 
<> physiotherapy. We will help you to relieve pain with certain techniques like heat therapy which also improves flexibility. You'll also get advices for activities and exercises that you can do. The total therapy will take about 1 and a half months and for full recovery you may need another two months. For scheduling the sessions, they will help you at the front desk.
- treatmentRecommendationIndex == 2:
~ prescribedTreatment = Prescription("Sur") 
<> surgery. Here, we either remove or repair the tissue by an open or mini-open operation depending on the damaged tissue. If it's severely damaged, we graft a tissue from another part of your body and replace the damaged part in your shoulder with an open surgery. You'll need to rest for a while after the procedure and full recovery could take 6 months. I'll be seeing you for your controls during this time. You can arrange a date at the front desk for the procedure.
- treatmentRecommendationIndex == 3:
~ prescribedTreatment = Prescription("WaS") 
<> wait and see option. This isn't exactly a treatment but sometimes our body can manage to recover in time without an intervention. It can be good to wait and see rather than an unnecessary intervention but of course in the mean time, you need to be careful about your routines. A careless waiting surely will make it worse. So, please be observer of your situation for about 6 months.
- treatmentRecommendationIndex == 4:
~ prescribedTreatment = Prescription("AS") 
<> advice sheets. This is a guide for certain excercises rather than a treatment. These exercises will help you to strengthen your muscles in the shoulder area and therefore, pains to relieve. In regular practice, it can be very efficient in your situation. Please, continue this exercises for 3 months.
- treatmentRecommendationIndex == 5:
~ prescribedTreatment = Prescription("NSA") 
<> NSAIDs (painkillers). These are anti-inflamatory medicines that reduce inflamation and pain they causing. I am sure you are familiar with them if you ever heard aspirin or ibuprofen. You'll need to use these for two weeks.
- treatmentRecommendationIndex == 6:
~ prescribedTreatment = Prescription("IaP") 
<> injection and physiotherapy. While injection helps to reduce inflamation and pain, physiotherapy afterwards helps you to strengthen your muscles. Together it's a more efficient option than they are taken seperately. Total procedure will take about 1 and a half months. You can get help at the fonrt desk for the arrangement of dates.
- treatmentRecommendationIndex == 7:
~ prescribedTreatment = Prescription("SaP") 
<> surgery with physiotherapy. Here, after we repaired or replaced the damaged tissue in your shoulder, you go under physical therapy. Surgery on its own can be painful sometimes but with physiotherapy, pain management is aimed while strengthening the shoulder. Together with therapy, total procedure could take about 4 months and full recovery can take upto 2 more months. They will help you at the front desk for the arrangement of the dates.
}
{
- storyFlow == IS:
->P_IS_1
- storyFlow == IRfC:
->P_IRfC_0
- storyFlow == BR:
->P_BR_0
- storyFlow == PfSDM:
    {
    - recommendInPfSDM == true:
    ->P_PfSDM_1A
    - else:
    ->P_PfSDM_0
    }
}


//*******Functions

==function GoToGTO==
~return 0

==function ChangeFocusTopics==
~return 0

==function RecommendTreatment==
~return 0

==function FinalizeSession==
~return 0

==function TakingFocus==
~return 0

==function Prescription(name)==
{
- name == "AS":
~return "Advice Sheets"
- name == "Sur":
~return "Surgery"
- name == "Phy":
~return "Physiotherapy"
- name == "NSA":
~return "NSAIDs"
- name == "Inj":
~return "Injection"
- name == "WaS":
~return "Wait and See"
- name == "IaP":
~return "Injection and Physiotherapy"
- name == "SaP":
~return "Surgery and Physiotherapy"
}







