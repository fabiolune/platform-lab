# get microsoft image with dotnet sdk 3.1
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

# install tool for code coverage collection
RUN dotnet tool install --global dotnet-reportgenerator-globaltool --version 4.8.8

# copy source project files
WORKDIR /app

## copy sln
COPY *.sln .

COPY ./Api/Api.csproj ./Api/Api.csproj
COPY ./Api.Unit.Tests/Api.Unit.Tests.csproj ./Api.Unit.Tests/Api.Unit.Tests.csproj
COPY ./Common/Common.csproj ./Common/Common.csproj
COPY ./Common.Unit.Tests/Common.Unit.Tests.csproj ./Common.Unit.Tests/Common.Unit.Tests.csproj
COPY ./Frontend/Frontend.csproj ./Frontend/Frontend.csproj
COPY ./Frontend.Unit.Tests/Frontend.Unit.Tests.csproj ./Frontend.Unit.Tests/Frontend.Unit.Tests.csproj

# nuget restore
RUN dotnet restore --runtime alpine-x64

# copy everything else (excluding content specified in .dockerignore)
COPY . .

# read configuration and store into env variables
ARG buildConfiguration=Debug
ENV BUILD_CONFIGURATION=${buildConfiguration}

# build solution and prepare assemblies for following tasks
RUN dotnet build *.sln --configuration ${buildConfiguration} --no-restore

ENV TEST_RESULTS_FOLDER=/app/test_results

RUN if [ -f .coverageignore ]; then echo "file exists"; else echo "" > .coverageignore; fi

ARG coverageExclude=[NUnit3.TestAdapter]*

RUN tr -d '\015' <.coverageignore >.coverageignore_temp \
  && rm -f .coverageignore \
  && mv .coverageignore_temp .coverageignore \
  && EXCLUDE_LIST=$(grep -v -e '^$\|^\s*\#' .coverageignore | tr "," "\n" | xargs -i echo "/app/"{} | paste -sd ",") \
  && find . -name \*Unit.Tests.csproj -print | xargs -i dotnet test {} \
  --no-build \
  --no-restore \
  --logger:trx \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput="./TestResults/opencover.xml" \
  /p:Exclude=\"${coverageExclude}\" \
  /p:ExcludeByFile="\"$EXCLUDE_LIST\"" \
  -r ${TEST_RESULTS_FOLDER} \
  --configuration ${BUILD_CONFIGURATION}

# merge all code coverage into a single report
ENV COVERAGE_MERGE_FOLDER=/app/test_coverage
RUN $HOME/.dotnet/tools/reportgenerator \
  "-reports:`find . -name \"opencover.xml\" | xargs echo | tr -s \" \" \";\"`" -targetdir:$COVERAGE_MERGE_FOLDER \
  "-reportTypes:TextSummary;Cobertura;lcov"

RUN find . -name \*Integration.Tests.csproj -print | xargs -i dotnet test {} \
  --no-build \
  --no-restore \
  --logger:trx \
  -r ${TEST_RESULTS_FOLDER} \
  --configuration ${BUILD_CONFIGURATION}

ARG version=0.0.0

# create entrypoint to export all tests results with coverage and packages to host
ENV OUTPUT_FOLDER=/app/_output
ENTRYPOINT cp -r ${COVERAGE_MERGE_FOLDER} ${OUTPUT_FOLDER} \
  && cp -r ${TEST_RESULTS_FOLDER} ${OUTPUT_FOLDER} \
  && chmod -R 777 ${OUTPUT_FOLDER}
