<img src="https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/raw/development/COMET-Community-Edition.png" width="250">

The CDP4-COMET-WEB Community Editition (CE) is the RHEA Group open source Concurrent Design web based application compliant with ECSS-E-TM-10-25 Annex A and Annex C. The solution provides the following items:
  - COMET.Web.Common: A Common Library for any Blazor based application related to ECSS-E-TM-10-25. This can be used to develop various ECSS-E-TM-10-25 web applications and is distributed with the APACHE 2.0 license.
  - COMET.Web.Common.Test: A Common Library that includes DevExpress Blazor and Tasks test helpers and is distributed with the APACHE 2.0 license.
  - COMETwebapp: The CDP4-COMET web application which depends on `COMET.Web.Common` and is distributed with the AGPL version 3.0 license.

A demo version of the web application is available at https://comet-web.cdp4.org

![GitHub issues](https://img.shields.io/github/issues/RHEAGROUP/COMET-WEB-Community-Edition.svg)

[![Publish Docker Container](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/publish-docker-container.yml/badge.svg)](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/publish-docker-container.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=coverage)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_COMET-WEB-Community-Edition&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_COMET-WEB-Community-Edition)


## Build Status

GitHub actions are used to build and test the software

Branch | Build Status
------- | :------------
Master | ![Build Status](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/CodeQuality.yml/badge.svg?branch=master)
Development | ![Build Status](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/CodeQuality.yml/badge.svg?branch=development)

> The CDP4-COMET-WEB SPA is automaticaly deployed to https://comet-web.cdp4.org using a [Github action](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/publish-docker-container.yml)

# CodeCov - Code Coverage

Branch      | Build Status
----------- | ------------
Master      | [![codecov](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition/branch/master/graph/badge.svg?token=2kfZrIOUtI)](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition)
Development | [![codecov](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition/branch/development/graph/badge.svg?token=2kfZrIOUtI)](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition)

## Concurrent Design

The Concurrent Design method is an approach to design activities in which all design disciplines and stakeholders are brought together to create an integrated design in a collaborative way of working.

The Concurrent Design method brings many advantages to the early design phase by providing a structure for this otherwise chaotic phase. Many design concepts have been implemented in the Concurrent Design method to help a team of stakeholders perform their task. The design work is done in collocated sessions with all stakeholders involved and present, creating an integrated design and enabling good communication and exchange of information between team members.

## Package Installation

The packages are available on Nuget at:

project                                                                         | Nuget
------------------------------------------------------------------------------- | ------------
[CDP4.WEB.Common](https://www.nuget.org/packages/CDP4.WEB.Common)             | [![NuGet Badge](https://buildstats.info/nuget/CDP4.WEB.Common)](https://buildstats.info/nuget/CDP4.WEB.Common)
[CDP4.WEB.Common.Test](https://www.nuget.org/packages/CDP4.WEB.Common.Test)   | [![NuGet Badge](https://buildstats.info/nuget/CDP4.WEB.Common.Test)](https://buildstats.info/nuget/CDP4.WEB.Common.Test)


## Web Application Build and Deploy using Docker - Manual

The CDP4-COMET-WEB SPA is built using docker and the result is a Docker container ready to be deployed (or pushed to Docker Hub). The Docker file is located in the COMETwebapp project folder.

> The Docker command needs to be executed from the commandline in the **solution** folder. Please note that the docker file is a multi-stage docker file. In the first stage the application is built using the private DevExpress nuget feed. In order to access this nuget feed, it is required to EXPORT the API-KEY to an environment variable.

```
$ ./solutionfolder# export DEVEXPRESS_NUGET_KEY=<YOUR-API-KEY>
$ ./solutionfolder# DOCKER_BUILDKIT=1 docker build --secret id=DEVEXPRESS_NUGET_KEY,env=DEVEXPRESS_NUGET_KEY -f COMETwebapp/Dockerfile -t rheagroup/comet-web-community-edition:latest -t rheagroup/comet-web-community-edition:<specific-version> .
$ ./solutionfolder# docker run -p 8080:80 --name comet-web rheagroup/comet-web-community-edition:latest
```

Push to docker hub

```
$ ./solutionfolder# docker push rheagroup/comet-web-community-edition:latest
$ ./solutionfolder# docker push rheagroup/comet-web-community-edition:<specific-version>
```

> The CDP4-COMET-WEB SPA is automaticaly deployed to https://comet-web.cdp4.org using a [Github action](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/publish-docker-container.yml)

## COMET-SDK

The CDP4-COMET-WEB Community Edition make use of the [COMET-SDK](https://github.com/RHEAGROUP/COMET-SDK-Community-Edition).

# License

The CDP4-COMET-WEB Community Edition is provided to the community under the GNU Affero General Public License. The COMET Community Edition relies on open source and proprietary licensed components. Some of these components have a license that is not compatible with the GPL or AGPL. For these components Additional permission under GNU GPL version 3 section 7 are granted. See the license files for the details. The license can be found [here](LICENSE).

The COMET.WEB.Common and COMET.WEB.Common.Test libraries (nuget packages) are provided to the community under the APACHE 2.0 License.

The [RHEA Group](https://www.rheagroup.com) also provides the [CDP4-COMET Web Services Enterprise Edition](https://github.com/RHEAGROUP/CDP4-WebServices-Community-Edition/wiki/CDP4-Web-Services-Enterprise-Edition) which comes with commercial support and more features. [Contact](https://www.rheagroup.com/contact) us for more details.

# Contributions

Contributions to the code-base are welcome. However, before we can accept your contributions we ask any contributor to sign the Contributor License Agreement (CLA) and send this digitaly signed to s.gerene@rheagroup.com. You can find the CLA's in the CLA folder.