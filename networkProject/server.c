#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <pthread.h>
#include <unistd.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>
#include "message.c"
#include <arpa/inet.h>

pthread_mutex_t queue_mutex;

struct twitterDBEntry {
	char cityName[15];
	char keywords[200];
};

struct client {
	int fd;
	int port;
	char *ip;
};

struct twitterDBEntry twitterDB[100];
int twitterDBCounter = 0;
int activeTaskCounter = 0;
int maxTasks = 0;
int currentTaskCounter = 0;
FILE *fp_queue = NULL;
int stop = 0;
struct client connectionQueue[1000];

void addToTwitterDB(struct twitterDBEntry entry)
{
	int i = 0;

	strncpy(twitterDB[twitterDBCounter].cityName, entry.cityName, strlen(entry.cityName));
	strncpy(twitterDB[twitterDBCounter++].keywords, entry.keywords, strlen(entry.keywords));
}

void lookupTwitterDB(char cityName[], char keywords[])
{
	int i = 0;
        for(i = 0; i < twitterDBCounter; i++) {
		if(!strncmp(twitterDB[i].cityName, cityName, strlen(cityName))) {
			strncpy(keywords, twitterDB[i].keywords,strlen(twitterDB[i].keywords));
			return;
		}
	}
	// not found error
	strncpy(keywords,"NA",2);
}

void printTwitterDB()
{
	int i = 0;
	for(i = 0; i < twitterDBCounter; i++) {
		printf("CityName: %s, TrendingKeyWords: %s\n",twitterDB[i].cityName, twitterDB[i].keywords);
	}
}

void populateTaskQueue(int port)
{

	//printf("Starting server\n");

	int sockfd, newsockfd, portno;
     socklen_t clilen;
     struct sockaddr_in serv_addr, cli_addr;
     sockfd = socket(AF_INET, SOCK_STREAM, 0);
     if (sockfd < 0) 
        error("ERROR opening socket");
     bzero((char *) &serv_addr, sizeof(serv_addr));
     portno = port;
     serv_addr.sin_family = AF_INET;
     serv_addr.sin_addr.s_addr = INADDR_ANY;
     serv_addr.sin_port = htons(portno);
     if (bind(sockfd, (struct sockaddr *) &serv_addr,
              sizeof(serv_addr)) < 0) 
              error("ERROR on binding");

     while (1){

     	printf("server listens\n");
	     listen(sockfd,5);
	     clilen = sizeof(cli_addr);
	     newsockfd = accept(sockfd, 
	                 (struct sockaddr *) &cli_addr, 
	                 &clilen);
	     if (newsockfd < 0) 
	          error("ERROR on accept");

	    printf("server accepts connection\n");
	    pthread_mutex_lock(&queue_mutex);

	    connectionQueue[activeTaskCounter].fd = newsockfd;
	    connectionQueue[activeTaskCounter].ip = inet_ntoa(cli_addr.sin_addr);
	    connectionQueue[activeTaskCounter].port = ntohs(cli_addr.sin_port);

	    activeTaskCounter++;
	    pthread_mutex_unlock(&queue_mutex);


	}

    // server socket
    close(sockfd);

}

void *run(void *arg)
{	
	int counter = 0;
	int tid = *(int *)arg; 
	char fname[15];
	int newsockfd;

	while(1) {
		pthread_mutex_lock(&queue_mutex);
		if (activeTaskCounter > currentTaskCounter) {
			counter = currentTaskCounter++;
			newsockfd = connectionQueue[counter].fd;
			pthread_mutex_unlock(&queue_mutex);

			printf ("Thread %d is handling client %s,%d\n", tid, connectionQueue[counter].ip, connectionQueue[counter].port); 

			struct MSG initReq;
		    initReq.code = 100;
		    strcpy(initReq.message, "");
		    initReq.length = 0;
		    printf("server sends handshaking: (100,0,)\n");
		    sendMSG(newsockfd, initReq);

			while (1){

		        // get request
		        int EOR = 0;
		        struct MSG req = getMSG(newsockfd);
		        struct MSG res;
		        
		        if (req.code == 101){
		            // received handshake
		            continue;
		            
		        } else if (req.code == 102){
		        	// received twitter request

		        	// LOOKUP KEYWORDS
					char keywords[200]="\0";
		        	lookupTwitterDB(req.message, keywords);
					keywords[strlen(keywords)]='\0';

		            res.code = 103;
		            strcpy(res.message, keywords);
		            res.length = strlen(res.message);
		            printf("server sends twitterTrend response: (103,%d,\"%s\")\n", res.length, res.message);
		        } else if (req.code == 104){
		            // received end of request
		            res.code = 105;
		            strcpy(res.message, "");
		            res.length = strlen(res.message);
		            EOR = 1;
		            printf("server sends end of response: (105,0,)\n");
		        }

		        // send response
		        sendMSG(newsockfd, res);

		        // break if end of request
		        if (EOR == 1) break;

		    }
		    // client socket
		    close(newsockfd);
		    printf("server closes the connection\n");
			
			printf ("Thread %d is finished handling client %s,%d\n", tid, connectionQueue[counter].ip, connectionQueue[counter].port); 
		
		} else {
			pthread_mutex_unlock(&queue_mutex);
		}
	}
	pthread_exit(NULL);
	return 0;
}	 	

int main(int argc, char *argv[])
{

	// read from Twitter trend, populate the TwitterDB
	FILE *fp = NULL;
	char buffer[120];
	struct twitterDBEntry entry;
	char *token;
	int i = 0;
	int numThreads = atoi(argv[2]);
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

	// Spawn num_threads threads
	for(i = 0; i < numThreads; i++)
	{
		tids[i]=i+1;
		pthread_create(&tid[i], NULL, run, (void *) &tids[i]);
	}


	int port = atoi(argv[1]);
	populateTaskQueue(port);

	// wait for threads before terminating
	for (i = 0; i < numThreads; i++) {
    		pthread_join(tid[i], NULL);
	}

	fclose(fp_queue);
	fp_queue = NULL;
	return 0;
}
