import {getToken} from './authHandler'
const backendUrl = import.meta.env.VITE_BACKEND_URL;

export interface Message{
    id: number;
    message: string;
    chatId: number;
    fromUser: number;
    fromUserId: number;
    date: string;
    isEdited: boolean;
    }

export interface ChatItem {
    id: number;
    content: string;
    timestamp: any;
    time: string;
    date: string;
    userName: string;
    userId: string;
    userPfp?: string;
    isActive: string;
    isEdited?:boolean;
    isDeleted?:boolean;
    isEvent?:boolean;
    withoutDetails?:boolean;
}

export async function fetchMessages(chatId: string, page: number, pageSize: number){
    let token = await getToken();
    
    const response = await fetch(`${backendUrl}/chat/getMessages?chatId=${chatId}&page=${page}&pageSize=${pageSize}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    
    const data = await response.json();
    
    
    if (response.ok) {
        if (data.chatItems) {
            const chatItems: ChatItem[] = await data.chatItems.map((item: any, index: number, array: any[]) => {
                const dateObject = new Date(item.timestamp);
                const date = dateObject.toLocaleDateString();
                const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);
                const timestamp = dateObject.getTime();
            
                let withoutDetails = false;
                if (index > 0) {
                    const previousItem = array[index - 1];
                    const previousTimestamp = new Date(previousItem.timestamp);
                    const diffInMinutes = (timestamp - previousTimestamp.getTime()) / (1000 * 60);
                    withoutDetails = item.userId === previousItem.userId && diffInMinutes < 5;
                }
            
                return {
                    id: item.id,
                    content: item.content,
                    timestamp: timestamp,
                    date: date,
                    time: time,
                    userName: item.userName,
                    userId: item.userId,
                    isActive: item.isActive,
                    isEdited: item.isEdited,
                    isDeleted: item.isDeleted,
                    isEvent: item.isEvent,
                    userPfp: `${backendUrl}${item.userPfp}`,
                    withoutDetails: withoutDetails
                };
            });
    
            
            return chatItems;
        } else {
            const chatItems: ChatItem[] = await data.chatItems.map((item: any) => ({
                id: "",
                content: "",
                timestamp: "",
                userName: "",
                isEdited: ""
            }));
            throw new Error("Empty array error!");
        }
        
    } else {
        throw new Error(data);
    }
};



export async function handleMessageSubmit(messageToSubmit: string, chatId: string){
    let token = await getToken();
    const requestBody = JSON.stringify({message: messageToSubmit, chatId: chatId});

    const response = await fetch(`${backendUrl}/chat/sendMessage`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody
    });

    if (!response.ok) {
        console.error('Error sending message:', await response.text());
    }
}

export async function handleEditMessage(chatId: string, messageId: number, newContent: string) {
    let token = await getToken();
    

    const response = await fetch(`${backendUrl}/chat/editMessage/${chatId}/${messageId}/${newContent}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    if (response.ok){
        return response;
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}
export async function handleDeleteMessage(chatId: string, messageId: number) {
    let token = await getToken();
    

    const response = await fetch(`${backendUrl}/chat/deleteMessage/${chatId}/${messageId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    if (response.ok){
        return response;
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}








