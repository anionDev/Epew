from ScriptCollection.TFCPS.DotNet.TFCPS_CodeUnitSpecific_DotNet import TFCPS_CodeUnitSpecific_DotNet_Functions,TFCPS_CodeUnitSpecific_DotNet_CLI
 
def build():

    tf:TFCPS_CodeUnitSpecific_DotNet_Functions=TFCPS_CodeUnitSpecific_DotNet_CLI.parse(__file__)
    tf.build(["win-x64","linux-x64"], False) 


if __name__ == "__main__":
    build()
