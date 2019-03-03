# Setup the project
1. From Visual Studio, creat a new project.
2. Command line

```
git init
git config --global credential.helper '!aws --profile sandboxuser1 codecommit credential-helper $@'   
git config --global credential.UseHttpPath true
```
3. SAM deploy
```
sam package --s3-bucket siri-sandbox-build --output-template xmlchecker-updated-serverless.template --template-file .\serverless.yml --profile sandboxuser1 --region us-east-1
sam deploy --template-file ./xmlchecker-updated-serverless.template --stack-name xmlchecker --capabilities CAPABILITY_IAM

dotnet lambda package-ci --s3-bucket siri-sandbox-build --output-template xmlchecker-updated-serverless.template --template serverless.yml  --profile sandboxuser1 --region us-east-1
```