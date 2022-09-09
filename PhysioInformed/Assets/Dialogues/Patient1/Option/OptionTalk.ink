INCLUDE GTO_AS
INCLUDE GTO_Sur
INCLUDE GTO_Phy
INCLUDE GTO_Inj
INCLUDE GTO_NSA
INCLUDE GTO_WaS
INCLUDE GTO_SaP
INCLUDE GTO_IaP

EXTERNAL StartKeypoints()
EXTERNAL ActivateKeypoints()
EXTERNAL DocTextView(status)

LIST speaker = (Doctor), Patient
LIST keypoints = PIAT, PEDT, RTfT

VAR keypointsCompleted = false
VAR nextStep = false
VAR secondOption = false 
VAR treatmentOrder = 0
VAR treatmentCount = 0
VAR choiceStatus = 0

VAR totalAvoidJar = 0
VAR totalSufInfo = 0
VAR totalApplyICE = 0

VAR finalAvoidJar = 0
VAR finalSufInfo = 0
VAR finalApplyICE = 0

==EndGTO==
~speaker = Doctor
- Okay. That was all for the options we have. Now we can move into deciding on your final treatment among these.
->DONE

==KeypointsCompleted==
{
- treatmentOrder == treatmentCount: 
~secondOption = true 
}
* [Proceed to the decision step]
~nextStep = true
->DONE
+ [Continue explaining options]
~keypointsCompleted = true
->DONE

==function DocTextView(status)==
~return 0

==function StartKeypoints==
~return 0

==function ActivateKeypoints==
~return 0

==function TotalOSCEPoints(x, y, z)==
~totalAvoidJar += x
//~finalAvoidJar += x

~totalSufInfo += y
//~finalSufInfo += y

~totalApplyICE += z
//~finalApplyICE += z



