#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>
#include <regex.h> 

static int DEBUG = 0;

void error(const char *msg)
{
    perror(msg);
    exit(1);
}

struct MSG {
    int code;
    int length;
    char message[512];
};

void sendMSG(int fh, struct MSG msg){
    char str[512];
    sprintf(str, "(%d,%d,%s)", msg.code, msg.length, msg.message);
    if (DEBUG) printf("Sending: %s\n", str);
    int n = write(fh,str,strlen(str));
    if (n < 0) error("ERROR writing to socket");
}

struct MSG parseReqest(char * buffer, int fh){
    struct MSG req;

    char status[4];
    char * msgLen;
    char msg[512];

    // check for invalid message
    int regexi;
    regex_t regex;
    regexi = regcomp(&regex, "^[(][0-9]+,[0-9]+,.*[)]$", REG_EXTENDED);
    regexi = regexec(&regex,buffer, 0, NULL, 0);
    if (regexi == REG_NOMATCH) {
        // invalid request, send 106 and exit
        struct MSG req;
        req.code = 106;
        strcpy(req.message, "Invalid request");
        req.length = strlen(req.message);;
        printf("Invalid message, sending 106: (106,%d,%s)\n",req.length, req.message);
        sendMSG(fh, req);
        close(fh);
        exit(0);
    }

    char *token = strtok(buffer, ",");

    strncpy( status, token + 1, 3);
    status[3] = '\0';
    req.code = atoi(status);

    token = strtok(NULL, ",");
    msgLen = token;
    req.length = atoi(msgLen);

    strncpy( msg, token + strlen(msgLen) + 1, req.length);
    msg[req.length] = '\0';

    strcpy(req.message, msg);

    return req;
}

struct MSG getMSG(int fh){
    char buffer[512];
    bzero(buffer,512);

    int n = read(fh,buffer,512);
    if (n < 0) error("ERROR reading from socket");

    struct MSG req = parseReqest(buffer, fh);
    if (DEBUG) printf("Got message: (%d, %d, %s)\n", req.code, req.length, req.message);

    // receive 106
    if (req.code == 106){
        printf("ERROR 106 received: %s\n", req.message);
        close(fh);
        exit(0);
    }

    if (req.code < 100 || req.code > 106){
        // invalid request, send 106 and exit
        struct MSG req;
        req.code = 106;
        strcpy(req.message, "Invalid code");
        req.length = strlen(req.message);
        printf("Invalid message, sending 106: (106,%d,%s)\n", req.length, req.message);
        sendMSG(fh, req);
        close(fh);
        exit(0);
    }

    return req;
}
