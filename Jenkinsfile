pipeline {
    agent any

    environment {
        AWS_REGION = 'ap-south-1'
        EB_APPLICATION_NAME = 'HealthAxisAPI4'
        EB_ENVIRONMENT_NAME = 'HealthAxisAPI4-dev'
        S3_BUCKET = 'healthaxis-deploy-162504351735-ap-south-1-an'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build Angular') {
            steps {
                dir('HealthAxis.UI') {
                    bat 'npm ci'
                    bat 'npm run build'
                }
            }
        }

        stage('Publish Blazor') {
            steps {
                bat 'dotnet publish HealthAxis.Admin\\HealthAxis.Admin.csproj -c Release -o blazor-publish-temp'
            }
        }

        stage('Copy Blazor into API wwwroot') {
            steps {
                bat 'if not exist HealthAxis.API\\wwwroot\\blazor mkdir HealthAxis.API\\wwwroot\\blazor'
                bat 'xcopy /E /Y /I blazor-publish-temp\\wwwroot\\* HealthAxis.API\\wwwroot\\blazor\\'
            }
        }

        stage('Publish API') {
            steps {
                bat 'dotnet publish HealthAxis.API\\HealthAxis.API.csproj -c Release -o publish'
            }
        }

        stage('Zip published output') {
    steps {
        dir('publish') {
            bat 'jar -cMf ../deploy-package.zip .'
        }
    }
}

        stage('Upload to S3 and Deploy to EB') {
            steps {
                withCredentials([[$class: 'AmazonWebServicesCredentialsBinding', credentialsId: 'aws-deploy-creds']]) {
                    bat "aws s3 cp deploy-package.zip s3://%S3_BUCKET%/deploy-package-%BUILD_NUMBER%.zip --region %AWS_REGION%"

                    bat "aws elasticbeanstalk create-application-version --application-name %EB_APPLICATION_NAME% --version-label v-%BUILD_NUMBER% --source-bundle S3Bucket=%S3_BUCKET%,S3Key=deploy-package-%BUILD_NUMBER%.zip --region %AWS_REGION%"

                    bat "aws elasticbeanstalk update-environment --environment-name %EB_ENVIRONMENT_NAME% --version-label v-%BUILD_NUMBER% --region %AWS_REGION%"
                }
            }
        }
    }
}