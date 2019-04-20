import httplib2
import ntpath
import urllib

def getFile(url, filePath):
    '''
    GET запрос
    '''
    if (url[-1] != "/"):
        url += "/"    
    filePath = urllib.parse.quote_plus(filePath)
    uri = url + filePath
    h = httplib2.Http(".cache")
    try:
        (resp_headers, content) = h.request(uri, "GET")
    except:
        print("Error! Can't send url request.")
        return
    if (resp_headers.status == 200):
        filename = ntpath.basename(filePath)
        f = open("./saved/{}".format(filename), "wb")
        f.write(content)
        f.close()   
        print("Saved: [{}]".format(ntpath.abspath("./saved/{}".format(filename))))
    elif(resp_headers.status == 404):
        print("Error! Bad request. File does not exists")
    else:
        print("Error in saving file.")
    return

def createFile(url, filename, contents):
    '''
    PUT запрос
    '''
    if (url[-1] != "/"):
        url += "/"
    filename = urllib.parse.quote_plus(filename)
    url += filename
    body = {"CONTENT": contents}
    h = httplib2.Http(".cache")
    try:
        (resp_headers, content) = h.request(url, method="PUT", body=urllib.parse.urlencode(body))
    except:
        print("Error! Can't send url request.")
        return
    if (resp_headers.status == 200):
        print("Sucсess")
    else:
        print("Error. File did not created")


def appendToFile(url, filename, contents):
    '''
    POST запрос
    '''
    if (url[-1] != "/"):
        url += "/"
    filename = urllib.parse.quote_plus(filename)
    url += filename
    body = {"CONTENT": contents}
    h = httplib2.Http(".cache")
    try:
        (resp_headers, content) = h.request(url, method="POST", body=urllib.parse.urlencode(body))
    except:
        print("Error! Can't send url request.")
        return
    if (resp_headers.status == 200):
        print("Sucсess.")
    else:
        print("Error. File did not appended")


def deleteFile(url, filename):
    '''
    DELETE запрос
    '''
    if (url[-1] != "/"):
        url += "/"
    filename = urllib.parse.quote_plus(filename)
    url += filename
    h = httplib2.Http(".cache")
    try:
        (resp_headers, content) = h.request(url, method="DELETE")
    except:
        print("Error! Can't send url request.")
        return
    if (resp_headers.status == 200):
        print("Deleted.")
    else:
        print("Error. File did not deleted")

def moveFile(url, filename, destFolder):
    '''
    MOVE запрос
    '''
    if (url[-1] != "/"):
        url += "/"
        
    filename = urllib.parse.quote_plus(filename)
    url += filename
    h = httplib2.Http(".cache")    
    body = {"DEST_FOLDER": destFolder}
    try:
        (resp_headers, content) = h.request(url, method="MOVE", body=urllib.parse.urlencode(body))
    except:
        print("Error! Can't send url request.")
        return
    if (resp_headers.status == 200):
        print("Moved.")
    else:
        print("Error. File did not moved")

def copyFile(url, filename, destFolder):
    '''
    COPY запрос
    '''
    if (url[-1] != "/"):
        url += "/"
    filename = urllib.parse.quote_plus(filename)
    url += filename
    h = httplib2.Http(".cache")    
    body = {"DEST_FOLDER": destFolder}
    try:
        (resp_headers, content) = h.request(url, method="COPY", body=urllib.parse.urlencode(body))
    except:
        print("Error! Can't send url request.")
        return
    if (resp_headers.status == 200):
        print("Copied.")
    else:
        print("Error. File did not copied")



#getFile("http://127.0.0.1:8080", "WebService.exe")
def main():
    choose = 1
    print("Please, enter url of web-service")
    url = input()
    while (choose != 0):
        print("Choose action:\n1 - get file\n2 - create file\n3 - append to file\n4 - delete file")
        print("5 - move file\n6 - copy file\n0 - to exit")
        choose = int(input())
        print("Good choice!")
        if (choose == 1):
            print("Please, enter file path relatived server work folder")
            path = input()
            getFile(url, path)
        elif(choose == 2):
            print("Please, enter filename relatived server work folder:")
            filename = input()
            print("Contents of the new file:")
            contents = input()
            createFile(url, filename, contents)
        elif(choose == 3):
            print("Please, enter filename relatived server work folder:")
            filename = input()
            print("Contents to add to file:")
            contents = input()
            appendToFile(url, filename, contents)
        elif(choose == 4):            
            print("Please, enter filename relatived server work folder:")
            filename = input()
            deleteFile(url, filename)
        elif(choose == 5):
            print("Please, enter filename relatived server work folder:")
            filename = input()
            print("Please, enter new folder relatived server work folder:")
            destFolder = input()
            moveFile(url, filename, destFolder)
        elif(choose == 6):
            print("Please, enter filename relatived server work folder:")
            filename = input()
            print("Please, enter new folder relatived server work folder:")
            destFolder = input()
            copyFile(url, filename, destFolder)
        

main()

