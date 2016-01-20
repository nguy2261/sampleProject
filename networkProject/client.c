

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <pthread.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h> 
#include "message.c"

pthread_mutex_t queue_mutex;

struct twitterDBEntry {
	char cityName[15];
	char keywords[200];
};

struct twitterDBEntry twitterDB[100];
int twitterDBCounter = 0;
char taskQueue[500][20];
int activeTaskCounter = 0;
int maxTasks = 0;
int currentTaskCounter = 0;
FILE *fp_queue = NULL;
int stop = 0;
int sockfd = 0;
char toFile[3000]="\0";

void addToTwitterDB(struct twitterDBEntry entry)
{
	int i = 0;

	strncpy(twitterDB[twitterDBCounter].cityName, entry.cityName, strlen(entry.cityName));
	strncpy(twitterDB[twitterDBCounter++].keywords, entry.keywords, strlen(entry.keywords));
}

void lookupTwitterDB(char cityName[], char keywords[])
{

	struct MSG req;
	
	// send request
	req.code = 102;
    strcpy(req.message, cityName);
    req.length = strlen(req.message);
	printf("client sends twitterTrend request: (102,%d,\"%s\")\n", req.length, cityName);
    sendMSG(sockfd, req);

    // get keywords
    struct MSG res = getMSG(sockfd);

    //printf("Got keywords: %s\n", res.message);
    strcpy(keywords, res.message);
    //printf("Client receives response (103,%d,\"%s\")\n", res.length, res.message);

}

void printTwitterDB()
{
	int i = 0;
	for(i = 0; i < twitterDBCounter; i++) {
		printf("CityName: %s, TrendingKeyWords: %s\n",twitterDB[i].cityName, twitterDB[i].keywords);
	}
}

void populateTaskQueue()
{
	char buffer[20];
        
        while ((activeTaskCounter < maxTasks) && ( fgets (buffer, sizeof(buffer), fp_queue)!=NULL)) {
                buffer[strlen(buffer)-1] = '\0';
                memset(taskQueue[activeTaskCounter], 0, sizeof(char)*20);
                strncpy(taskQueue[activeTaskCounter],buffer,strlen(buffer));
                activeTaskCounter++;
        }
	if(activeTaskCounter < maxTasks) {
		stop = 1;
	}
	else {
		//printf("\nTask queue full.. waiting\n");
		stop = 0;
	}
}

void printTaskQueue()
{
	int i = 0;
	printf("\nActive Tasks: \n");
	for(i=0;i<activeTaskCounter;i++) {
		printf("%s\n", taskQueue[i]);
	}
}

/* 
* Given a fname, 
* 1. Open the file to get the city name
* 2. Look-up the city name in twitterDB and get corresponding trending keywords
* 3. Write keywords to fname_result
*/
 
void processTask(char *fname)
{
	char buffer[15];
	char keywords[200]="\0";
	

	FILE *fp = fopen(fname, "r");
    if(fp == NULL)
    {
            printf("\nError opening file: %s", fname);
            exit(1);
    }

    if (fgets (buffer, sizeof(buffer), fp)!=NULL) {
		buffer[strlen(buffer)-1] = '\0';
		lookupTwitterDB(buffer, keywords);
		keywords[strlen(keywords)]='\0';
		strcat(toFile, buffer);
		strcat(toFile, " : ");
		strcat(toFile, keywords);
		strcat(toFile, "\n");
		
		fclose(fp);		

	}
	else {
		fclose(fp);
		printf("\nError reading from file: %s", fname);
		exit(1);
	}	
}

void run()
{	
	int counter = 0;
	int tid = 1; 
	char fname[15];
	while(1) {
		if (activeTaskCounter > currentTaskCounter) {
			counter = currentTaskCounter++;
			strcpy(fname,taskQueue[counter]);

			processTask(fname);
		
			if(currentTaskCounter == activeTaskCounter) {
				activeTaskCounter = 0;
				currentTaskCounter = 0;
				if (fp_queue) {
					populateTaskQueue();
				}
			}
		}
		else if (stop == 1) {
			break;
		}
	}
}

void startNetworking(int argc, char *argv[]){


	int portno, n;
    struct sockaddr_in serv_addr;
    struct hostent *server;

    char buffer[256];
    if (argc < 3) {
       fprintf(stderr,"usage %s hostname port\n", argv[0]);
       exit(0);
    }
    portno = atoi(argv[2]);
    sockfd = socket(AF_INET, SOCK_STREAM, 0);
    if (sockfd < 0) 
        error("ERROR opening socket");
    server = gethostbyname(argv[1]);
    if (server == NULL) {
        fprintf(stderr,"ERROR, no such host\n");
        exit(0);
    }
    bzero((char *) &serv_addr, sizeof(serv_addr));
    serv_addr.sin_family = AF_INET;
    bcopy((char *)server->h_addr, 
         (char *)&serv_addr.sin_addr.s_addr,
         server->h_length);
    serv_addr.sin_port = htons(portno);
    if (connect(sockfd,(struct sockaddr *) &serv_addr,sizeof(serv_addr)) < 0) 
        error("ERROR connecting");
    printf("client connects\n");

    

    while (1){

        // get request
        int EOR = 0;
        struct MSG res = getMSG(sockfd);
        struct MSG req;
        
        if (res.code == 100){
        	printf("client sends handshake response: (101,0,)\n");
            req.code = 101;
		    strcpy(res.message, "");
		    req.length = 0;
		    sendMSG(sockfd, req);
            break;
        } 
    }
    
    //printf("Done\n");

    //close(sockfd);
}	 	

int main(int argc, char *argv[])
{

	// NETWORKING
	startNetworking(argc, argv);

	// read from Twitter trend, populate the TwitterDB
	FILE *fp = NULL;
	char buffer[120];
	struct twitterDBEntry entry;
	char *token;
	int i = 0;
	int numThreads = 1;
	pthread_t tid[numThreads];
	int tids[numThreads];
	maxTasks = numThreads;

	// ./twitterTrend client_file num_threads
	fp = fopen("TwitterDB.txt", "r");
	if(fp == NULL)
	{
		printf("\nError opening file: TwitterDB.txt");
		exit(1);
	}

	while (fgets (buffer, sizeof(buffer), fp)!=NULL) { 
		token = strtok(buffer, ",");
		strncpy(entry.cityName, token, strlen(token));
		entry.cityName[strlen(token)]='\0';
		token = strtok(NULL, " ");
		strncpy(entry.keywords, token, strlen(token));
		entry.keywords[strlen(token)-1]='\0';
		addToTwitterDB(entry);
	}
	fclose(fp);


	// open client file and populate task queue 
	for (i = 0; i < argc -3; i++){
		//printf("running: %s\n", argv[i+3]);
		fp_queue = fopen(argv[i+3],"r");
		if(fp_queue == NULL)
	        {
	                printf("\nError opening file: %s", argv[1]);
	                exit(1);
	        }
		populateTaskQueue();
		toFile[0] = '\0';
		run();
		//printf("done with: %s\n : %s", argv[i+3], toFile);

		char resultFileName[23]="\0";
		strncpy(resultFileName,argv[i+3],strlen(argv[i+3]));
		strcat(resultFileName,".result");
		FILE *fpWrite = fopen(resultFileName, "w");
		if(fpWrite == NULL)
		{
			printf("\nError opening file: %s", resultFileName);
			exit(1);
		}
		fputs(toFile, fpWrite);
		fclose(fpWrite);

	}


	// close connection like a responsible young man
	printf("client sends end of request: (104,0,)\n");
	struct MSG req;
	req.code = 104;
    strcpy(req.message, "");
    req.length = strlen(req.message);
	sendMSG(sockfd, req);
	close(sockfd);

	// terminate
	fclose(fp_queue);
	fp_queue = NULL;
	return 0;
}
