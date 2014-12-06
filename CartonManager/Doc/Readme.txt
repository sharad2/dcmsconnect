By Sharad 5 Jul 2012.

To create a new custom view for editing cartons:

1. Create class QualifyRulesSample derived from CartonModel. In the constructor of this class, set the default values of all qualification rules.
2. Create class UpdateRulesSample derived from CartonModel. In the constructor of this class, set the default values of all update rules.
3. Create class SampleViewModel derived from ViewModelBase<QualifyRulesSample, UpdateRulesSample>. Add properties to this class which are needed by your custom UI.
4. Create the view sample.cshtml which takes SampleViewModel as the view model.
5. Provide a button to post your view to the Action UpdateCartonOrPallet.

Postback vs AJAX call to UpdateCartonOrPallet
---------------------------------------------

Sharad 8 Aug 2012: Sounds
-------------------------
Desktop pages should use HTML 5 audio tag as exemplified in PalletizeUi.cshtml. Code should alert if HTML5 capability is not available.

Looks like QUicktime does not support .wav files. We should be using .mp3 files.

Check scenarios:
 - No sound plugin available.
 - Using Media Player
 - Using QuickTime
 - Behavior in IE8 which does not support HTML5.


