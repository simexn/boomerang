<script lang="ts">
    import { friendsStore } from "$lib/stores/friendsStore";
    import { sentRequestsStore, receivedRequestsStore } from "$lib/stores/friendRequestsStore";
    import { userStatuses } from "$lib/stores/userStatusesStore";
    import { handleAcceptRequest, handleRejectRequest, handleRemoveFriend } from "$lib/Handlers/userHandler";
    import {goto} from "$app/navigation";
    
    $: console.log(filter)

    let userOptionsDropdown:any;
    let filter = "All";
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
    $: filteredFriends = filter === 'All' 
        ? $friendsStore
        : $friendsStore.filter(friend => $userStatuses[friend.id] === filter);
</script>
<svelte:window on:click={() => userOptionsDropdown = null} />
<div class="home-container">
    <div class="header">
        <div class="friends-span">
            <span>Friends</span>
        </div>
        <div class="button-container">
            <button class="header-button" on:click={() => setFilter('online')}>Online</button>
            <button class="header-button" on:click={() => setFilter('All')}>All</button>
            <button class="header-button" on:click={() => setFilter('Pending')}>Pending</button>
            <button class="header-button" on:click={() => setFilter('Requests')}>Requests</button>
            <button class="header-button" on:click={() => setFilter('Blocked')}>Blocked</button>
            <button class="header-button add-friend">Add Friend</button>
        </div>
    </div>
    <div class="searchbar">
        <input class="searchbar-input" placeholder="Search">
    </div>
    <div class="section-title-container">
        <p class="section-title">{filter}</p>
    </div>
    <div class="users-list">
        {#if filter === 'All' || filter === 'online'}
            {#each filteredFriends as friend, index (index)}
                <div class="user-item">
                    <div class="user-wrap">
                        <div class="user-icon">
                            <img src={friend.userPfp} alt="user icon" class="user-icon">
                        </div>
                        <div class="user-info">
                            <span>{friend.username}</span>
                            <div class="user-note">
                                <span>so real</span>
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
                                <button class="dropdown-item" on:click={()=>removeFriend(friend.id)}>Unfriend</button>
                                <button class="dropdown-item">Block</button>
                            </div>
                        {/if}
                    </div>
                </div>
            {/each}
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
                            <span>"so real"</span>
                        </div>
                    </div>
                </div>
                <div class="user-actions">
                    <i class="fa-solid fa-x" on:click={() => handleRejectRequest(request.username)}></i>
                </div>
            </div>
        {/each}
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
                                <span>"so real"</span>
                            </div>
                        </div>
                    </div>
                    <div class="user-actions">
                        <i class="fa-solid fa-check" on:click={() => acceptRequest(request.username)}></i>
                        <i class="fa-solid fa-x" on:click={() => rejectRequest(request.username)}></i>
                    </div>
                </div>
            {/each}
        {:else if filter === 'Blocked'}
            <div class="user-item">
                <div class="user-wrap">
                    <div class="user-icon">
                        <img src="https://via.placeholder.com/150" alt="user icon" class="user-icon">
                    </div>
                    <div class="user-info">
                        <span>Friend Request</span>
                        <div class="user-note">
                            <span>so real</span>
                        </div>
                    </div>
                </div>
                <div class="user-actions">
                    <i class="fa-solid fa-message"></i>
                    <i class="fas fa-ellipsis-v"></i>
                </div>
            </div>
        {/if}
    </div>
    <!-- List of friends goes here -->
</div>

<style>
.home-container {
    width:100%;
    
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
}
    .header span{
        font-size: 1.2rem;
        line-height: 1.25;
        font-weight: 600;
    }
.header-button {
    padding-left: 1rem;
    margin-left: 0.5rem;
    padding-right: 1rem;
    margin-right: 0.5rem;
    cursor: pointer;
    border-radius: 5px;
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
    font-size: 1rem;
}
.dropdown-menu{
                position: absolute;
                top: 100%; /* This positions the dropdown right under the user div */
                right: 0;
                z-index: 3;
            }

@media (max-width: 600px) {

    .friends-span {
        border-right: none;
        border-bottom: 1px solid rgba(var(--bg-sec), 0.08);
        padding: 0.5rem 0;
    }

    .button-container {
        overflow-x: scroll;
        padding: 0.5rem 0;
    }

    .header-button {
        padding: 0.5rem;
        margin: 0.25rem;
    }
}
</style>