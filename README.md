<img src="https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/raw/development/COMET-Community-Edition.png" width="250">

The COMET WEB Community Editition (CE) is the RHEA Group open source Concurrent Design web based application compliant with ECSS-E-TM-10-25 Annex A and Annex C.

A demo version of the application is available at https://comet-web.cdp4.org

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

GitHub actions are used to build and test the libraries

Branch | Build Status
------- | :------------
Master | ![Build Status](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/CodeQuality.yml/badge.svg?branch=master)
Development | ![Build Status](https://github.com/RHEAGROUP/COMET-WEB-Community-Edition/actions/workflows/CodeQuality.yml/badge.svg?branch=development)

# CodeCov - Code Coverage

Branch      | Build Status
----------- | ------------
Master      | [![codecov](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition/branch/master/graph/badge.svg?token=2kfZrIOUtI)](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition)
Development | [![codecov](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition/branch/development/graph/badge.svg?token=2kfZrIOUtI)](https://codecov.io/gh/RHEAGROUP/COMET-WEB-Community-Edition)

## Concurrent Design

The Concurrent Design method is an approach to design activities in which all design disciplines and stakeholders are brought together to create an integrated design in a collaborative way of working.

The Concurrent Design method brings many advantages to the early design phase by providing a structure for this otherwise chaotic phase. Many design concepts have been implemented in the Concurrent Design method to help a team of stakeholders perform their task. The design work is done in collocated sessions with all stakeholders involved and present, creating an integrated design and enabling good communication and exchange of information between team members.

## Build and Deploy using Docker

The COMET-WEB SPA is built using docker and the result is a Docker container ready to be deployed (or pushed to Docker Hub). The Docker file is located in the COMETwebapp project folder.


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

## COMET-SDK

The COMET-WEB Community Edition make use of the [COMET-SDK](https://github.com/RHEAGROUP/COMET-SDK-Community-Edition).

# License

The COMET-WEB Community Edition is provided to the community under the GNU Affero General Public License. The COMET Community Edition relies on open source and proprietary licensed components. Some of these components have a license that is not compatible with the GPL or AGPL. For these components Additional permission under GNU GPL version 3 section 7 are granted. See the license files for the details. The license can be found [here](LICENSE).

The [RHEA Group](https://www.rheagroup.com) also provides the [COMET Web Services Enterprise Edition](https://github.com/RHEAGROUP/CDP4-WebServices-Community-Edition/wiki/CDP4-Web-Services-Enterprise-Edition) which comes with commercial support and more features. [Contact](https://www.rheagroup.com/contact) us for more details.

# Contributions

Contributions to the code-base are welcome. However, before we can accept your contributions we ask any contributor to sign the Contributor License Agreement (CLA) and send this digitaly signed to s.gerene@rheagroup.com. You can find the CLA's in the CLA folder.