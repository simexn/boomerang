// import { HubConnectionBuilder } from '@microsoft/signalr';

// export async function setupConnection(chatId, chatItems, groupInfo, userInfo, scrollContainer, backendUrl) {
//     if (connection) {
//         await connection.stop();
//     }

    

//     connection = new HubConnectionBuilder()
//         .withUrl(`${backendUrl}/chatHub`)
//         .build();

//         connection.on("ReceiveMessage", async function(data: any){

//             const dateObject = new Date();
//             const date = dateObject.toLocaleDateString();
//             const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

//             let chatItemToAdd:ChatItem = {
//                 id: data.id,
//                 content: data.text,
//                 timestamp: Date.now().toLocaleString(),
//                 date: date,
//                 time: time,
//                 userName: data.fromUser.userName
//             };

//             chatItems = [...chatItems, chatItemToAdd];

//             await tick();
//             scrollContainer.scrollTop = scrollContainer.scrollHeight;
//         });

//         connection.on("UserJoined", async function(user: any) {
//             groupInfo.users = [...groupInfo.users, user];

//             groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

//             const dateObject = new Date();
//             const date = dateObject.toLocaleDateString();
//             const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

//             let chatItemToAdd:ChatItem = {
//                 id: Date.now(), // Use the current timestamp as a temporary ID
//                 content: 'UserJoined',
//                 timestamp: Date.now().toLocaleString(),
//                 date: date,
//                 time: time,
//                 userName: user.userName
//             };

//             chatItems = [...chatItems, chatItemToAdd];

//             await tick();
//             scrollContainer.scrollTop = scrollContainer.scrollHeight;
//         });


//     connection.on("UserLeft", async function(user: any) {
//         groupInfo.users = groupInfo.users.filter(u => u.id !== user.id);
//         groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

//         const dateObject = new Date();
//         const date = dateObject.toLocaleDateString();
//         const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

//         let chatItemToAdd:ChatItem = {
//             id: Date.now(), // Use the current timestamp as a temporary ID
//             content: 'UserLeft',
//             timestamp: Date.now().toLocaleString(),
//             date: date,
//             time: time,
//             userName: user.userName
//         };

//             chatItems = [...chatItems, chatItemToAdd];

//             await tick();
//             scrollContainer.scrollTop = scrollContainer.scrollHeight;
//     });

//     connection.on("UserKicked", async function(user: any) {
//         if (user.id === userInfo.id) {
//             goto('/chat/home');
            
//         }
//         groupInfo.users = groupInfo.users.filter(u => u.id !== user.id);

//         const dateObject = new Date();
//         const date = dateObject.toLocaleDateString();
//         const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

//         let chatItemToAdd:ChatItem = {
//             id: Date.now(), // Use the current timestamp as a temporary ID
//             content: 'UserKicked',
//             timestamp: Date.now().toLocaleString(),
//             date: date,
//             time: time,
//             userName: user.userName
//         };

//             chatItems = [...chatItems, chatItemToAdd];

//             await tick();
//             scrollContainer.scrollTop = scrollContainer.scrollHeight;
//     });
    
//     connection.on("UserBanned", async function(user: any){
//         if (user.id === userInfo.id) {
//             goto('/some-other-page');
//         }
//     });

//     connection.on("UserPromoted", async function(user: any){
//         groupInfo.admins = [...groupInfo.admins, user];

//         const dateObject = new Date();
//         const date = dateObject.toLocaleDateString();
//         const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

//         let chatItemToAdd:ChatItem = {
//             id: Date.now(), // Use the current timestamp as a temporary ID
//             content: 'UserPromoted',
//             timestamp: Date.now().toLocaleString(),
//             date: date,
//             time: time,
//             userName: user.userName
//         };

//         chatItems = [...chatItems, chatItemToAdd];

//         await tick();
//         scrollContainer.scrollTop = scrollContainer.scrollHeight;
//     });

//     connection.on("UserDemoted", async function(user: any){
//         groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

//         const dateObject = new Date();
//         const date = dateObject.toLocaleDateString();
//         const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

//         let chatItemToAdd:ChatItem = {
//             id: Date.now(), // Use the current timestamp as a temporary ID
//             content: 'UserDemoted',
//             timestamp: Date.now().toLocaleString(),
//             date: date,
//             time: time,
//             userName: user.userName
//         };

//         chatItems = [...chatItems, chatItemToAdd];

//         await tick();
//         scrollContainer.scrollTop = scrollContainer.scrollHeight;
//     });

//     connection.on("OwnershipTransferred", async function(user: any){
//         groupInfo.creatorId = user.id;
//         groupInfo.admins = [...groupInfo.admins, user];

//         const dateObject = new Date();
//         const date = dateObject.toLocaleDateString();
//         const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

//         let chatItemToAdd:ChatItem = {
//             id: Date.now(), // Use the current timestamp as a temporary ID
//             content: 'OwnershipTransferred',
//             timestamp: Date.now().toLocaleString(),
//             date: date,
//             time: time,
//             userName: user.userName
//         };

//         chatItems = [...chatItems, chatItemToAdd];

//         await tick();
//         scrollContainer.scrollTop = scrollContainer.scrollHeight;
    
//     });

//     connection.on("EditedMessage", async function(data: any){
//         chatItems = chatItems.map(item => item.id === data.id ? {...item, content: data.text, isEdited:true} : item);
//     });
//     connection.on("DeletedMessage", async function(data: any){
//         chatItems = chatItems.map(item => item.id === data.id ? {...item, isDeleted:true} : item);
//     });
    
//     await connection.start()
//         .then(function(){
//             connection.invoke('getConnectionId')
//             .then(async function(connectionId: string){
//                 _connectionId = connectionId;
//                 await loadMessages();
//                 await getGroupInfo();
//                 await joinRoom();
//                 await tick();
//                 scrollContainer.scrollTop = scrollContainer.scrollHeight;
//             })
//         })
//         .catch(function(err: any){
//             console.error(err.toString());
//         });
// }

// async function joinRoom() {
//     let token = await getToken();

//     const response = await fetch(`${backendUrl}/chat/joinRoom`, {
//         method: 'POST',
//         headers: {
//             'Authorization': `Bearer ${token}`,
//             'Content-Type': 'application/json'
//         },
//         body: JSON.stringify({
//             connectionId: _connectionId,
//             roomName: chatId
//         })
//     });

//     if (!response.ok) {
//         console.error('Error joining room:', await response.text());
//     } 
// }