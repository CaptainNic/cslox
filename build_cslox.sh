#!/bin/sh

NC='\033[0m'
RED='\033[1;31m'
GRN='\033[1;32m'
YLW='\033[1;33m'

CURRENT_STAGE=""

START_STAGE()
{
    CURRENT_STAGE=$1
    printf "${YLW}[  START ]${NC} ${CURRENT_STAGE}\n"
}

END_STAGE()
{
    if [ $? -neq 0 ]; then
        stageFailed $?
    else
        stageComplete
    fi
}

stageComplete()
{
    printf "${GRN}[   OK   ]${NC} ${CURRENT_STAGE}\n"
    CURRENT_STAGE=""
}

stageFailed()
{
    printf "${RED}[ FAILED ]${NC} ${CURRENT_STAGE} - err: ${1}\n"
    CURRENT_STAGE=""
    buildFailed
}

BUILD_FAILED()
{
    printf "${RED}\nBuild Failed\n\n"
}

BUILD_OK()
{
    printf "${GRN}\nBuild Complete\n\n"
}

### START MAIN ###

printf "Building CsLox\n"

START_STAGE "BUILD - GenerateAst"
dotnet build -c Release Tools/GenerateAst/GenerateAst.csproj
END_STAGE

START_STAGE "BUILD - CsLox"
dotnet build -c Release CsLox/CsLox.csproj
END_STAGE

BUILD_OK