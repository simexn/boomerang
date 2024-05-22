<script lang="ts">
    import { friendsStore } from "$lib/stores/friendsStore";
    import { sentRequestsStore, receivedRequestsStore, blockedUsersStore } from "$lib/stores/friendRequestsStore";
    import { userStatuses } from "$lib/stores/userStatusesStore";
    import { handleAcceptRequest, handleAddFriend, handleBlockUser, handleRejectRequest, handleRemoveFriend, handleSearchUser, handleUnblockUser } from "$lib/handlers/userHandler";
    import {goto} from "$app/navigation";
    import { onMount } from "svelte";
    import type { User } from "$lib/handlers/accountHandler";
    const backendUrl = import.meta.env.VITE_BACKEND_URL;    
    $: console.log(filter)

    let userOptionsDropdown:any;
    let isGradientRemoved = false;
    let friendUsername: string;
    let friendToAdd: User | null;
    let friendToAddError: string = '';
    let friendToAddSuccess: string ='';
    let filter = "All";
    let searchTerm = '';
    const filterNamesInBulgarian: any = {
        'All': 'Всички',
        'online': 'Активни',
        'Pending': 'Изчакващи',
        'Requests': 'Заявки',
        'Blocked': 'Блокирани',
        'add': 'Добавяне на приятел',
    };
    $: filterName = filterNamesInBulgarian[filter];

    onMount(() => {
        const buttonContainer: any = document.getElementById('button-container');

        buttonContainer.addEventListener('scroll', () => {
            if (buttonContainer.scrollLeft + buttonContainer.offsetWidth >= buttonContainer.scrollWidth) {
                isGradientRemoved = true;
            } else {
                isGradientRemoved = false;
            }
        });
    });
    
    function setFilter(value: string) {
        filter = value;
    }

    async function removeFriend(friendId: string){
        await handleRemoveFriend(friendId);
    }

    async function acceptRequest(username: string){
            await handleAcceptRequest(username)
        
    }
    async function rejectRequest(username: string){
        await handleRejectRequest(username);
    }

    async function blockUser(userId: string){
        await handleBlockUser(userId);
    }
    async function unblockUser(userId: string){
        await handleUnblockUser(userId);
    }
    async function searchFriend(username: string){
        const response = await handleSearchUser(username);
        if (response.ok){
            friendToAddError='';
            const data = await response.json();

            friendToAdd = {
                id: data.id,
                userName: data.username,
                email: data.email,
                accountCreated: data.accountCreated,
                profilePictureUrl: backendUrl + data.profilePictureUrl,
                isAdmin: data.isAdmin
            }
        }
        else{
            friendToAddError = await response.text();
        }
    }
    async function addFriend(username: string){
        const response = await handleAddFriend(username);
        if (typeof response === 'string') {
            friendToAddError = response;
        }
        else{
            friendToAddSuccess = 'Поканата за приятелство беше изпратена успешно.';
            friendToAdd= null;}
    }

    
    $: filteredFriends = filter === 'All' 
        ? $friendsStore.filter(friend => friend.username.toLowerCase().includes(searchTerm.toLowerCase()))
        : $friendsStore.filter(friend => ($userStatuses[friend.id] === filter || $userStatuses[friend.id] === 'away') && friend.username.toLowerCase().includes(searchTerm.toLowerCase()));
</script>
<svelte:window on:click={() => userOptionsDropdown = null} />
<div class="home-container">
    <div class="header">
        <div class="friends-span">
            <span>Приятели</span>
        </div>
        <div class="button-container" id="button-container">
            <button class="header-button" on:click={() => setFilter('online')}>Активни</button>
            <button class="header-button" on:click={() => setFilter('All')}>Всички</button>
            <button class="header-button" on:click={() => setFilter('Pending')}>Изчакващи</button>
            <button class="header-button" on:click={() => setFilter('Requests')}>Заявки</button>
            <button class="header-button" on:click={() => setFilter('Blocked')}>Блокирани</button>
            <button class="header-button add-friend" on:click={() => setFilter('add')}>Добавяне на приятел</button>
        </div>
    </div>
    {#if filter != "add"}
    <div class="searchbar">
        <input class="searchbar-input" placeholder="Търсене" bind:value={searchTerm}>
    </div>
    <div class="section-title-container">
        <p class="section-title">{filterName}</p>
    </div>
    {/if}
    <div class="users-list">
        {#if filter === 'All' || filter === 'online'}
            
            {#each filteredFriends as friend, index (index)}
                <div class="user-item">
                    <div class="user-wrap">
                        <div class="user-icon">
                            <img src={friend.userPfp} alt="user icon" class="user-icon">
                            <span class="user-status-dot" class:online={$userStatuses[friend?.id.toString()] == 'online'} class:away={$userStatuses[friend?.id.toString()] == 'away'}></span>
                        </div>
                        <div class="user-info">
                            <span>{friend.username}</span>
                            <div class="user-note">
                                <span></span>
                            </div>
                        </div>
                    </div>
                    <div class="user-actions">
                        <i class="fa-solid fa-message" on:click={()=> goto(`/chat/me/${friend.chatId}`)}></i>
                        <!-- svelte-ignore a11y-no-static-element-interactions -->
                        <!-- svelte-ignore a11y-click-events-have-key-events -->
                        <i class="fas fa-ellipsis-v" on:click|stopPropagation={()=> userOptionsDropdown = userOptionsDropdown == friend.id ? null : friend.id}></i>
                        {#if userOptionsDropdown == friend.id}
                            <div role="navigation" class="dropdown-menu show">
                                <button class="dropdown-item" on:click={()=>removeFriend(friend.id)}>Премахване на приятел</button>
                                <button class="dropdown-item" on:click={() => blockUser(friend.id)}>Блокиране</button>
                            </div>
                        {/if}
                    </div>
                </div>
            {/each}
            {#if filteredFriends.length === 0 && filter == "All"}
                <p class="empty-message">Нямате приятели. Може да добавите чрез "Добавяне на приятел" бутона.</p>
            {:else if filteredFriends.length === 0 && filter == "online"}
                <p class="empty-message">Нямате приятели, които в момента са на линия.</p>
            {/if}
        {:else if filter === 'Pending'}
        {#each $sentRequestsStore as request, index (index)}
            <div class="user-item">
                <div class="user-wrap">
                    <div class="user-icon">
                        <img src={request.userPfp} alt="user icon" class="user-icon">
                    </div>
                    <div class="user-info">
                        <span>{request.username}</span>
                        <div class="user-note">
                            <span>{request.requestSentDate}</span>
                        </div>
                    </div>
                </div>
                <div class="user-actions">
                    <i class="fa-solid fa-x" on:click={() => handleRejectRequest(request.username)}></i>
                </div>
            </div>
        {/each}
        {#if $sentRequestsStore.length === 0}
            <p class="empty-message">Нямате изпратени заявки за приятелство.</p>
        {/if}
        {:else if filter === 'Requests'}
            {#each $receivedRequestsStore as request, index (index)}
                <div class="user-item">
                    <div class="user-wrap">
                        <div class="user-icon">
                            <img src={request.userPfp} alt="user icon" class="user-icon">
                        </div>
                        <div class="user-info">
                            <span>{request.username}</span>
                            <div class="user-note">
                                <span>Получена на: {request.requestSentDate}</span>
                            </div>
                        </div>
                    </div>
                    <div class="user-actions">
                        <i class="fa-solid fa-check" on:click={() => acceptRequest(request.username)}></i>
                        <i class="fa-solid fa-x" on:click={() => rejectRequest(request.username)}></i>
                    </div>
                </div>
            {/each}
            {#if $receivedRequestsStore.length === 0}
                <p class="empty-message">Нямате получени заявки за приятелство.</p>
            {/if}
        {:else if filter === 'Blocked'}
            {#each $blockedUsersStore as blockedUser, index (index)}
                <div class="user-item">
                    <div class="user-wrap">
                        <div class="user-icon">
                            <img src={blockedUser.userPfp} alt="user icon" class="user-icon">
                        </div>
                        <div class="user-info">
                            <span>{blockedUser.username}</span>
                            <div class="user-note">
                                <span>Блокиран на: {blockedUser?.requestRespondedDate}</span>
                            </div>
                        </div>
                    </div>
                    <div class="user-actions">
                        <i class="fa-solid fa-x" on:click={() => unblockUser(blockedUser.id)}></i>
                    </div>
                </div>
            {/each}
            {#if $blockedUsersStore.length === 0}
                <p class="empty-message">Няма потребители, които сте блокирали.</p>
            {/if}
        {:else if filter === 'add'}
        <div class="add-friend-container">
            <h2>Добавяне на приятел</h2>
            <p>Пратете покана за приятелство, използвайки потребителското име на приятеля Ви.</p>
            <div class="add-friend-input" class:red-outline={friendToAddError}>
                <input type="text" placeholder="Потребителско име" bind:value={friendUsername}/>
                <button on:click={() => searchFriend(friendUsername)}>Търсене на потребител</button>
            </div>
            <p class="text-danger error-message">{friendToAddError}</p>
            <p class="text-success success-message">{friendToAddSuccess}</p>
        </div>
        {#if friendToAdd}
        <div class="user-item">
            <div class="user-wrap">
                <div class="user-icon">
                    <img src={friendToAdd?.profilePictureUrl} alt="user icon" class="user-icon">
                </div>
                <div class="user-info">
                    <span>{friendToAdd?.userName}</span>
                    <div class="user-note">
                        <span>Член на бумеранг от: {friendToAdd?.accountCreated}</span>
                    </div>
                </div>
            </div>
            <div class="user-actions">
                <i class="fa-solid fa-user-plus" on:click={() => { if (friendToAdd?.userName) addFriend(friendToAdd.userName); }}></i>
            </div>
        </div>
        {/if}
        {/if}
    </div>
</div>

<style>
    .user-status-dot {
    height: 10px;
    width: 10px;
    background-color: #bbb;
    border-radius: 50%;
    border: 1px solid black;
    display: inline-block;
    position: absolute;
    bottom: 2px;
    right: 2px;
    }
    .user-status-dot.online {
    background-color: #4CAF50;
    }
    .user-status-dot.away{
        background-color: #FFC107;
    
    }
    .add-friend-container {
    display: flex;
    flex-direction: column;
    align-items:start;
    justify-content: start;
    padding: 20px;
}

.add-friend-container h2 {
    font-size: 1.5em;
    margin-bottom: 10px;
}

.add-friend-container p {
    font-size: 1em;
    margin-bottom: 20px;
}

.add-friend-input {
    position: relative;
    width: 100%;
}

.add-friend-input input {
    width: 100%;
    padding: 10px;
    border-radius: 10px;
    border: none;
    background: var(--prim-fg);
    
}
.add-friend-input input:focus {
        outline: none; /* Remove the default outline */
        border: none; /* Remove the border */
    }

.add-friend-input button {
    position: absolute;
    right: 0;
    top: 0;
    height: 100%;
    padding: 10px 20px;
    border-top-right-radius: 10px;
    border-bottom-right-radius: 10px;
    border: none;
    background-color: var(--prim-tp);
    color: #ffffff;
    cursor: pointer;
}
.red-outline{
        border-color: red !important;
        border-radius: 10px;
        box-shadow: 0 0 0 0.25rem rgba(255,0,0,.25) !important;
        transition: border-color .15s ease-in-out,box-shadow .15s ease-in-out;
    }
.home-container {
    width:100%; 
}
.empty-message{
    font-style: italic;
    font-size: 1.6rem;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #9e9e9e;
    overflow: hidden;
}

.header {
    display: flex;
    flex-direction: row;
    padding-top: 1rem;
    padding-bottom: 1rem;
    height: 3.9rem;
    overflow: hidden;
    align-items: center; 
    border-bottom: 1px solid rgba(var(--bg-sec), 0.08);
    position: relative;
    transition: opacity 0.3s ease-in-out;
}
    .header span{
        font-size: 1.2rem;
        line-height: 1.25;
        font-weight: 600;
    }
.button-container{
    position: relative;
    overflow-x: auto;
    white-space: nowrap;
}
     
.header-button {
    padding-left: 1rem;
    margin-left: 0.5rem;
    padding-right: 1rem;
    margin-right: 0.5rem;
    cursor: pointer;
    border:none;
    background: none;
    border-radius: 5px;
    display: inline-block;
}
    .add-friend {
        background-color: #2dc770;
        color:white;
    }

.header-button:hover {
    background-color: var(--hover-dark);
}

.friends-span{
    border-right: 1px solid rgba(var(--bg-sec), 0.08);
    padding-right: 1rem;
    padding-left: 1rem;
}

.searchbar{
    margin: 1rem 1.2rem 0.5rem 2rem;
    flex: none;
    background: var(--prim-fg);
    box-sizing: border-box;
    position: relative;
    flex: 1 1 auto;
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    padding: 1px;
    min-width: 0;
    align-items: center;
    border-radius: 5px;  
}

.searchbar-input{
    box-sizing: border-box;
    background: transparent;
    border: none;
    resize: none;
    flex: 1;
    min-width: 48px;
    margin: 1px;
    -webkit-appearance: none;
    -moz-appearance: none;
    appearance: none;

}

.section-title{  
    margin: 1rem 1.2rem 0.5rem 1.8rem;
    color: var(--prim-fg);
    box-sizing: border-box;
    text-overflow: ellipsis;
    white-space: nowrap;
    overflow: hidden;
    text-transform: uppercase;
    font-size: 12px;
    line-height: 16px;
    letter-spacing: .02em;
    font-weight: 600;
    flex: 1 1 auto;
    color: var(--channels-default);
}

.users-list{
    padding-bottom: 0.5rem;
    margin-top: 0.5rem;
}
.user-item{
    display: flex;
    flex-direction: row;
    margin-left: 30px;
    margin-right: 20px;
    font-weight: 500;
    font-size: 16px;
    line-height: 20px;
    
    box-sizing: border-box;
    border-top: 1px solid var(--prim-fg);
    padding-top: 0.4rem;
    padding-bottom: 0.4rem;
    justify-content: space-between;
}

.user-wrap{
    display: flex;
    overflow: hidden;
    align-items: center;
}
.user-icon{
    width: 32px;
    height: 32px;
    margin-right: 0.8rem;
    position: relative;
}
    .user-icon img{
        width: 100%;
        height: 100%;
        border-radius: 50%;
    
    }
.user-info{
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.user-actions{
    display: flex;
    margin-left: 0.5rem;
    justify-content: center;
    align-items: center;
    position: relative;
    
}
.user-actions i{
    padding-top: 0.75rem;
    padding-left: 0.75rem;
    margin-left: 0.5rem;
    padding-bottom:0.75rem;
    padding-right: 0.75rem;
    margin-right: 0.5rem;
    cursor: pointer;
    border-radius: 5px;
    width: 2rem; /* Add this line */
    height: 2rem; /* Add this line */
    display: flex; /* Add this line */
    justify-content: center; /* Add this line */
    align-items: center; /* Add this line */
}
        .user-actions i:hover{
            background-color: var(--hover-dark);
        }

.user-note{
    white-space: nowrap;
    text-overflow: ellipsis;
    overflow: hidden;
    font-size: 0.9rem;
}
.dropdown-menu{
                position: absolute;
                top: 100%; /* This positions the dropdown right under the user div */
                right: 0;
                z-index: 3;
            }

@media (max-width: 600px) {

    .friends-span {
        width:8rem;
        padding-left:0.5rem;
        padding-right: 0.5rem;
    }

    .button-container {
        padding: 0.5rem 0;
    }

    .header-button {
        padding: 0.5rem;
        margin: 0.25rem;
    }
}
</style>